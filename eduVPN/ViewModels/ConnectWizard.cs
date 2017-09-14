﻿/*
    eduVPN - End-user friendly VPN

    Copyright: 2017, The Commons Conservancy eduVPN Programme
    SPDX-License-Identifier: GPL-3.0+
*/

using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace eduVPN.ViewModels
{
    /// <summary>
    /// Connect wizard
    /// </summary>
    public class ConnectWizard : Window
    {
        #region Fields

        /// <summary>
        /// Instance directory URI IDs as used in <c>Properties.Settings.Default</c> collection
        /// </summary>
        private static readonly string[] _instance_directory_id = new string[]
        {
            null,
            "SecureInternet",
            "InstituteAccess",
        };

        /// <summary>
        /// The alpha factor to increase/decrease popularity
        /// </summary>
        private static readonly float _popularity_alpha = 0.1f;

        #endregion

        #region Properties

        /// <summary>
        /// Available instance sources
        /// </summary>
        public Models.InstanceSourceInfo[] InstanceSources
        {
            get { return _instance_sources; }
        }
        private Models.InstanceSourceInfo[] _instance_sources;

        /// <summary>
        /// VPN configuration histories
        /// </summary>
        public ObservableCollection<Models.VPNConfiguration>[] ConfigurationHistories
        {
            get { return _configuration_histories; }
        }
        private ObservableCollection<Models.VPNConfiguration>[] _configuration_histories;

        /// <summary>
        /// Selected instance source
        /// </summary>
        public Models.InstanceSourceType InstanceSourceType
        {
            get { return _instance_source_type; }
            set
            {
                if (value != _instance_source_type)
                {
                    _instance_source_type = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged("InstanceSource");
                    RaisePropertyChanged("AuthenticatingInstanceSelectPage");
                    RaisePropertyChanged("ConnectingProfileSelectPage");
                }
            }
        }
        private Models.InstanceSourceType _instance_source_type;

        /// <summary>
        /// Selected instance source
        /// </summary>
        public Models.InstanceSourceInfo InstanceSource
        {
            get { return InstanceSources[(int)_instance_source_type]; }
        }

        /// <summary>
        /// Authenticating eduVPN instance
        /// </summary>
        /// <remarks><c>null</c> if none selected.</remarks>
        public Models.InstanceInfo AuthenticatingInstance
        {
            get { return _authenticating_instance; }
            set { if (value != _authenticating_instance) { _authenticating_instance = value; RaisePropertyChanged(); } }
        }
        private Models.InstanceInfo _authenticating_instance;

        /// <summary>
        /// VPN session queue - session 0 is the active session
        /// </summary>
        public ObservableCollection<VPNSession> Sessions
        {
            get { return _sessions; }
        }
        private ObservableCollection<VPNSession> _sessions;

        /// <summary>
        /// Connection info command
        /// </summary>
        public DelegateCommand SessionInfo
        {
            get
            {
                if (_session_info == null)
                    _session_info = new DelegateCommand(
                        // execute
                        () => NavigateTo.Execute(StatusPage),

                        // canExecute
                        () => Sessions.Count > 0 && NavigateTo.CanExecute(StatusPage));

                return _session_info;
            }
        }
        private DelegateCommand _session_info;

        /// <summary>
        /// Navigate to page command
        /// </summary>
        public DelegateCommand<ConnectWizardPage> NavigateTo
        {
            get
            {
                if (_navigate_to == null)
                    _navigate_to = new DelegateCommand<ConnectWizardPage>(
                        // execute
                        page =>
                        {
                            ChangeTaskCount(+1);
                            try { CurrentPage = page; }
                            catch (Exception ex) { Error = ex; }
                            finally { ChangeTaskCount(-1); }
                        });

                return _navigate_to;
            }
        }
        private DelegateCommand<ConnectWizardPage> _navigate_to;

        /// <summary>
        /// Starts VPN session
        /// </summary>
        public DelegateCommand<StartSessionParams> StartSession
        {
            get
            {
                if (_start_session == null)
                    _start_session = new DelegateCommand<StartSessionParams>(
                        // execute
                        param =>
                        {
                            // Switch to the status page, for user to see the progress.
                            CurrentPage = StatusPage;

                            // Note: Sessions locking is not required, since all queue manipulation is done exclusively in the UI thread.

                            if (Sessions.Count > 0 && Sessions[Sessions.Count - 1].Configuration.Equals(param.Configuration))
                            {
                                // Wizard is already running (or scheduled to run) a VPN session of the same configuration as specified.
                                return;
                            }

                            // Launch the VPN session in the background.
                            new Thread(new ThreadStart(
                                () =>
                                {
                                    Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => ChangeTaskCount(+1)));
                                    try
                                    {
                                        // Create our new session.
                                        using (var session = new OpenVPNSession(this, param.Configuration))
                                        {
                                            VPNSession previous_session = null;
                                            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(
                                                () =>
                                                {
                                                    if (Sessions.Count > 0)
                                                    {
                                                        // Trigger disconnection of the previous session.
                                                        previous_session = Sessions[Sessions.Count - 1];
                                                        if (previous_session.Disconnect.CanExecute())
                                                            previous_session.Disconnect.Execute();
                                                    }

                                                    // Add our session to the queue.
                                                    Sessions.Add(session);
                                                    SessionInfo.RaiseCanExecuteChanged();
                                                }));
                                            try
                                            {
                                                if (previous_session != null)
                                                {
                                                    // Await for the previous session to finish.
                                                    if (WaitHandle.WaitAny(new WaitHandle[] { Abort.Token.WaitHandle, previous_session.Finished }) == 0)
                                                        throw new OperationCanceledException();
                                                }

                                                // Run our session.
                                                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => ChangeTaskCount(-1)));
                                                try { session.Run(); }
                                                finally { Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => ChangeTaskCount(+1))); }
                                            }
                                            finally
                                            {
                                                // Remove our session from the queue.
                                                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(
                                                    () =>
                                                    {
                                                        Sessions.Remove(session);
                                                        SessionInfo.RaiseCanExecuteChanged();

                                                        if (Sessions.Count <= 0 && CurrentPage == StatusPage)
                                                        {
                                                            // No more sessions and user is still on the status page. Redirect the wizard back.
                                                            CurrentPage = RecentConfigurationSelectPage;
                                                        }
                                                    }));
                                            }
                                        }
                                    }
                                    catch (OperationCanceledException) { }
                                    catch (Exception ex) { Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => Error = ex)); }
                                    finally { Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => ChangeTaskCount(-1))); }
                                })).Start();

                            // Do the configuration history book-keeping.
                            var configuration_history = ConfigurationHistories[(int)param.InstanceSourceType];
                            if (InstanceSources[(int)param.InstanceSourceType] is Models.LocalInstanceSourceInfo)
                            {
                                // Check for session configuration duplicates and update popularity.
                                int found = -1;
                                for (var i = configuration_history.Count; i-- > 0;)
                                {
                                    if (configuration_history[i].Equals(param.Configuration))
                                    {
                                        if (found < 0)
                                        {
                                            // Upvote popularity.
                                            configuration_history[i].Popularity = configuration_history[i].Popularity * (1.0f - _popularity_alpha) + 1.0f * _popularity_alpha;
                                            found = i;
                                        }
                                        else
                                        {
                                            // We found a match second time. This happened early in the Alpha stage when duplicate checking didn't work right.
                                            // Clean the list. The victim is less popular entry.
                                            if (configuration_history[i].Popularity < configuration_history[found].Popularity)
                                            {
                                                configuration_history.RemoveAt(i);
                                                found--;
                                            }
                                            else
                                            {
                                                param.Configuration.Popularity = configuration_history[i].Popularity;
                                                configuration_history.RemoveAt(found);
                                                found = i;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Downvote popularity.
                                        configuration_history[i].Popularity = configuration_history[i].Popularity * (1.0f - _popularity_alpha) /*+ 0.0f * _popularity_alpha*/;
                                    }
                                }

                                if (found < 0)
                                {
                                    // Add session configuration to history.
                                    configuration_history.Add(param.Configuration);
                                }
                            }
                            else if (
                                InstanceSources[(int)param.InstanceSourceType] is Models.DistributedInstanceSourceInfo ||
                                InstanceSources[(int)param.InstanceSourceType] is Models.FederatedInstanceSourceInfo)
                            {
                                // Set session configuration to history.
                                if (configuration_history.Count == 0)
                                    configuration_history.Add(param.Configuration);
                                else
                                    configuration_history[0] = param.Configuration;
                            }
                            else
                                throw new InvalidOperationException();

                            // Update settings.
                            var hist = new Models.VPNConfigurationSettingsList(configuration_history.Count);
                            foreach (var cfg in configuration_history)
                                hist.Add(cfg.ToSettings(InstanceSources[(int)param.InstanceSourceType].GetType()));
                            Properties.Settings.Default[_instance_directory_id[(int)param.InstanceSourceType] + "ConfigHistory"] = hist;
                        },

                        // canExecute
                        param => param != null && param.Configuration != null);

                return _start_session;
            }
        }
        private DelegateCommand<StartSessionParams> _start_session;

        /// <summary>
        /// StartSession command parameter set
        /// </summary>
        public class StartSessionParams
        {
            #region Properties

            /// <summary>
            /// Instance source
            /// </summary>
            public Models.InstanceSourceType InstanceSourceType { get; }

            /// <summary>
            /// VPN configuration
            /// </summary>
            public Models.VPNConfiguration Configuration { get; }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructs a StartSession command parameter set
            /// </summary>
            /// <param name="instance_source_type">Instance source type</param>
            /// <param name="configuration">VPN configuration</param>
            public StartSessionParams(Models.InstanceSourceType instance_source_type, Models.VPNConfiguration configuration)
            {
                InstanceSourceType = instance_source_type;
                Configuration = configuration;
            }

            #endregion
        }

        /// <summary>
        /// Instance request authorization event
        /// </summary>
        public event EventHandler<RequestInstanceAuthorizationEventArgs> RequestInstanceAuthorization;

        #region Pages

        /// <summary>
        /// The page the wizard is currently displaying
        /// </summary>
        public ConnectWizardPage CurrentPage
        {
            get { return _current_page; }
            set
            {
                if (value != _current_page)
                {
                    _previous_page = _current_page;
                    RaisePropertyChanged("PreviousPage");

                    _current_page = value;
                    RaisePropertyChanged();
                    _current_page.OnActivate();
                }
            }
        }
        private ConnectWizardPage _current_page;

        /// <summary>
        /// The previous page of the wizard
        /// </summary>
        public ConnectWizardPage PreviousPage
        {
            get { return _previous_page; }
        }
        private ConnectWizardPage _previous_page;

        /// <summary>
        /// The first page of the wizard
        /// </summary>
        public ConnectWizardPage StartingPage
        {
            get
            {
                if (_configuration_histories[(int)Models.InstanceSourceType.SecureInternet].Count > 0 ||
                    _configuration_histories[(int)Models.InstanceSourceType.InstituteAccess].Count > 0)
                    return RecentConfigurationSelectPage;
                else
                    return InstanceSourceSelectPage;
            }
        }

        /// <summary>
        /// Initializing wizard page
        /// </summary>
        public InitializingPage InitializingPage
        {
            get
            {
                if (_initializing_page == null)
                    _initializing_page = new InitializingPage(this);
                return _initializing_page;
            }
        }
        private InitializingPage _initializing_page;

        /// <summary>
        /// Instance source page
        /// </summary>
        public InstanceSourceSelectPage InstanceSourceSelectPage
        {
            get
            {
                if (_instance_source_page == null)
                    _instance_source_page = new InstanceSourceSelectPage(this);
                return _instance_source_page;
            }
        }
        private InstanceSourceSelectPage _instance_source_page;

        /// <summary>
        /// Authenticating instance selection page
        /// </summary>
        /// <remarks>Available only when authenticating and connecting instances can be different. I.e. <c>InstanceSource</c> is <c>eduVPN.Models.LocalInstanceSourceInfo</c> or <c>eduVPN.Models.DistributedInstanceSourceInfo</c>.</remarks>
        public AuthenticatingInstanceSelectPage AuthenticatingInstanceSelectPage
        {
            get
            {
                if (InstanceSource is Models.LocalInstanceSourceInfo ||
                    InstanceSource is Models.DistributedInstanceSourceInfo)
                {
                    // Only local and distrubuted authentication sources have this page.
                    // However, this page varies between Secure Internet and Institute Access.
                    switch (InstanceSourceType)
                    {
                        case Models.InstanceSourceType.SecureInternet:
                            if (_authenticating_country_select_page == null)
                                _authenticating_country_select_page = new AuthenticatingCountrySelectPage(this);
                            return _authenticating_country_select_page;

                        case Models.InstanceSourceType.InstituteAccess:
                            if (_authenticating_institute_select_page == null)
                                _authenticating_institute_select_page = new AuthenticatingInstituteSelectPage(this);
                            return _authenticating_institute_select_page;

                        default:
                            throw new InvalidOperationException();
                    }
                }
                else
                    throw new InvalidOperationException();
            }
        }
        private AuthenticatingCountrySelectPage _authenticating_country_select_page;
        private AuthenticatingInstituteSelectPage _authenticating_institute_select_page;

        /// <summary>
        /// Custom instance source page
        /// </summary>
        public CustomInstancePage CustomInstancePage
        {
            get
            {
                if (_custom_instance_page == null)
                    _custom_instance_page = new CustomInstancePage(this);
                return _custom_instance_page;
            }
        }
        private CustomInstancePage _custom_instance_page;

        /// <summary>
        /// (Instance and) profile selection wizard page
        /// </summary>
        /// <remarks>When <c>InstanceSource</c> is <c>eduVPN.Models.LocalInstanceSourceInfo</c> the profile selection page is returned; otherwise, the instance and profile selection page is returned.</remarks>
        public ConnectingInstanceAndProfileSelectBasePage ConnectingProfileSelectPage
        {
            get
            {
                if (InstanceSource is Models.LocalInstanceSourceInfo)
                {
                    // Profile selection (local authentication).
                    if (_connecting_profile_select_page == null)
                        _connecting_profile_select_page = new ConnectingProfileSelectPage(this);
                    return _connecting_profile_select_page;
                }
                else
                {
                    // Connecting instance and profile selection (distributed and federated authentication).
                    if (_connecting_instance_and_profile_select_page == null)
                        _connecting_instance_and_profile_select_page = new ConnectingInstanceAndProfileSelectPage(this);
                    return _connecting_instance_and_profile_select_page;
                }
            }
        }
        private ConnectingProfileSelectPage _connecting_profile_select_page;
        private ConnectingInstanceAndProfileSelectPage _connecting_instance_and_profile_select_page;

        /// <summary>
        /// Recent profile selection wizard page
        /// </summary>
        public RecentConfigurationSelectPage RecentConfigurationSelectPage
        {
            get
            {
                if (_recent_configuration_select_page == null)
                    _recent_configuration_select_page = new RecentConfigurationSelectPage(this);
                return _recent_configuration_select_page;
            }
        }
        private RecentConfigurationSelectPage _recent_configuration_select_page;

        /// <summary>
        /// Status wizard page
        /// </summary>
        public StatusPage StatusPage
        {
            get
            {
                if (_status_page == null)
                    _status_page = new StatusPage(this);
                return _status_page;
            }
        }
        private StatusPage _status_page;

        /// <summary>
        /// Settings wizard page
        /// </summary>
        public SettingsPage SettingsPage
        {
            get
            {
                if (_settings_page == null)
                    _settings_page = new SettingsPage(this);
                return _settings_page;
            }
        }
        private SettingsPage _settings_page;

        /// <summary>
        /// About wizard page
        /// </summary>
        public AboutPage AboutPage
        {
            get
            {
                if (_about_page == null)
                    _about_page = new AboutPage(this);
                return _about_page;
            }
        }
        private AboutPage _about_page;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs the wizard
        /// </summary>
        public ConnectWizard()
        {
            if (Properties.Settings.Default.SettingsVersion == 0)
            {
                // Migrate settings from previous version.
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.SettingsVersion = 1;
            }

            Dispatcher.ShutdownStarted += (object sender, EventArgs e) =>
            {
                // Persist settings to disk.
                Properties.Settings.Default.Save();
            };

            // Create session queue.
            _sessions = new ObservableCollection<VPNSession>();

            // Show initializing wizard page.
            CurrentPage = InitializingPage;

            // Setup initialization.
            var worker = new BackgroundWorker() { WorkerReportsProgress = true };
            worker.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                var source_type_length = (int)Models.InstanceSourceType._end;
                _instance_sources = new Models.InstanceSourceInfo[source_type_length];
                _configuration_histories = new ObservableCollection<Models.VPNConfiguration>[source_type_length];

                // Setup progress feedback. Each instance will add 10 ticks of progress.
                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => InitializingPage.Progress = new Range<int>(0, (source_type_length - (int)Models.InstanceSourceType._start) * 10, 0)));

                // Spawn instance source loading threads.
                Parallel.For((int)Models.InstanceSourceType._start, source_type_length, source_index =>
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => ChangeTaskCount(+1)));
                    try
                    {
                        for (;;)
                        {
                            int ticks = 0;
                            try
                            {
                                var response_cache = (JSON.Response)Properties.Settings.Default[_instance_directory_id[source_index] + "DiscoveryCache"];

                                // Get instance source.
                                var response_web = JSON.Response.Get(
                                    uri: new Uri((string)Properties.Settings.Default[_instance_directory_id[source_index] + "Discovery"]),
                                    pub_key: Convert.FromBase64String((string)Properties.Settings.Default[_instance_directory_id[source_index] + "DiscoveryPubKey"]),
                                    ct: Abort.Token,
                                    previous: response_cache);

                                // Add 5/10 ticks.
                                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => InitializingPage.Progress.Value += 5));
                                ticks += 5;

                                // Parse instance source JSON.
                                var obj_web = (Dictionary<string, object>)eduJSON.Parser.Parse(
                                    response_web.Value,
                                    Abort.Token);

                                if (response_web.IsFresh)
                                {
                                    if (response_cache != null)
                                    {
                                        try
                                        {
                                            // Verify sequence.
                                            var obj_cache = (Dictionary<string, object>)eduJSON.Parser.Parse(
                                                response_cache.Value,
                                                Abort.Token);

                                            bool rollback = false;
                                            try { rollback = (uint)eduJSON.Parser.GetValue<int>(obj_cache, "seq") > (uint)eduJSON.Parser.GetValue<int>(obj_web, "seq"); }
                                            catch { rollback = true; }
                                            if (rollback)
                                            {
                                                // Sequence rollback detected. Revert to cached version.
                                                obj_web = obj_cache;
                                                response_web = response_cache;
                                            }
                                        }
                                        catch { }
                                    }

                                    // Save response to cache.
                                    Properties.Settings.Default[_instance_directory_id[source_index] + "DiscoveryCache"] = response_web;
                                }

                                // Add 2/10 ticks.
                                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => InitializingPage.Progress.Value += 2));
                                ticks += 2;

                                // Load instance source.
                                _instance_sources[source_index] = Models.InstanceSourceInfo.FromJSON(obj_web);

                                // Attach to RequestAuthorization instance events.
                                foreach (var instance in _instance_sources[source_index])
                                    instance.RequestAuthorization += Instance_RequestAuthorization;

                                _configuration_histories[source_index] = new ObservableCollection<Models.VPNConfiguration>();

                                // Load configuration histories from settings.
                                var hist = (Models.VPNConfigurationSettingsList)Properties.Settings.Default[_instance_directory_id[source_index] + "ConfigHistory"];
                                Parallel.ForEach(hist,
                                    h =>
                                    {
                                        var cfg = new Models.VPNConfiguration();

                                        try
                                        {
                                            // Restore configuration.
                                            if (_instance_sources[source_index] is Models.LocalInstanceSourceInfo instance_source_local &&
                                                h is Models.LocalVPNConfigurationSettings h_local)
                                            {
                                                // Local authenticating instance source:
                                                // - Restore instance, which is both: authenticating and connecting.
                                                // - Restore profile.
                                                cfg.AuthenticatingInstance = instance_source_local.Where(inst => inst.Base.AbsoluteUri == h_local.Instance.Base.AbsoluteUri).FirstOrDefault();
                                                if (cfg.AuthenticatingInstance == null)
                                                {
                                                    cfg.AuthenticatingInstance = h_local.Instance;
                                                    cfg.AuthenticatingInstance.RequestAuthorization += Instance_RequestAuthorization;
                                                }
                                                cfg.ConnectingInstance = cfg.AuthenticatingInstance;

                                                // Don't try to match profile with existing profiles, or risk triggering OAuth in GetProfileList(). Use stored version from settings directly.
                                                //cfg.ConnectingProfile = cfg.ConnectingInstance.GetProfileList(cfg.AuthenticatingInstance, Abort.Token).Where(p => p.ID == h_local.Profile.ID).FirstOrDefault();
                                                cfg.ConnectingProfile = h_local.Profile;
                                                if (cfg.ConnectingProfile == null) return;
                                            }
                                            else if (_instance_sources[source_index] is Models.DistributedInstanceSourceInfo instance_source_distributed &&
                                                h is Models.DistributedVPNConfigurationSettings h_distributed)
                                            {
                                                // Distributed authenticating instance source:
                                                // - Restore authenticating instance.
                                                // - Restore last connected instance (optional).
                                                cfg.AuthenticatingInstance = instance_source_distributed.Where(inst => inst.Base.AbsoluteUri == h_distributed.AuthenticatingInstance).FirstOrDefault();
                                                if (cfg.AuthenticatingInstance == null) return;
                                                cfg.ConnectingInstance = instance_source_distributed.Where(inst => inst.Base.AbsoluteUri == h_distributed.LastInstance).FirstOrDefault();
                                            }
                                            else if (_instance_sources[source_index] is Models.FederatedInstanceSourceInfo instance_source_federated &&
                                                h is Models.FederatedVPNConfigurationSettings h_federated)
                                            {
                                                // Federated authenticating instance source:
                                                // - Get authenticating instance from federated instance source settings.
                                                // - Restore last connected instance (optional).
                                                cfg.AuthenticatingInstance = new Models.InstanceInfo(instance_source_federated);
                                                cfg.AuthenticatingInstance.RequestAuthorization += Instance_RequestAuthorization;
                                                cfg.ConnectingInstance = instance_source_federated.Where(inst => inst.Base.AbsoluteUri == h_federated.LastInstance).FirstOrDefault();
                                            }
                                            else
                                                return;

                                            cfg.Popularity = h.Popularity;
                                        }
                                        catch { return; }

                                        // Configuration successfuly restored. Add it.
                                        lock (_configuration_histories[source_index])
                                            _configuration_histories[source_index].Add(cfg);
                                    });

                                // Add 3/10 ticks.
                                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => InitializingPage.Progress.Value += 3));
                                ticks += 3;

                                break;
                            }
                            catch (OperationCanceledException) { break; }
                            catch (Exception ex)
                            {
                                // Do not re-throw the exception.
                                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                                {
                                    // Notify the sender the instance source loading failed.
                                    // This will overwrite all previous error messages.
                                    Error = new AggregateException(String.Format(Resources.Strings.ErrorInstanceSourceInfoLoad, _instance_directory_id[source_index]), ex);

                                    // Revert progress indicator value.
                                    InitializingPage.Progress.Value -= ticks;
                                }));
                            }

                            // Sleep for 3s, then retry.
                            if (Abort.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(3)))
                                break;
                        }
                    }
                    finally { Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => ChangeTaskCount(-1))); }
                });
            };

            // Setup initialization completition.
            worker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
            {
                ChangeTaskCount(+1);
                try
                {
                    if (Abort.Token.IsCancellationRequested)
                        return;

                    // Proceed to the "first" page.
                    RaisePropertyChanged("StartingPage");
                    CurrentPage = StartingPage;
                }
                finally { ChangeTaskCount(-1); }

                // Self-dispose.
                (sender as BackgroundWorker)?.Dispose();
            };

            worker.RunWorkerAsync();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when an instance requests user authorization
        /// </summary>
        /// <param name="sender">Instance of type <c>eduVPN.Models.InstanceInfo</c> requiring authorization</param>
        /// <param name="e">Authorization request event arguments</param>
        public void Instance_RequestAuthorization(object sender, Models.RequestAuthorizationEventArgs e)
        {
            // Re-raise this event as ConnectWizard event, to simplify view.
            // This way the view can listen ConnectWizard for authentication events only.
            if (Dispatcher.CurrentDispatcher == Dispatcher)
            {
                // We're in the GUI thread.
                var e_instance = new RequestInstanceAuthorizationEventArgs((Models.InstanceInfo)sender);
                RequestInstanceAuthorization?.Invoke(this, e_instance);
                e.AccessToken = e_instance.AccessToken;
            }
            else
            {
                // We're in the background thread - raise event via dispatcher.
                Dispatcher.Invoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        var e_instance = new RequestInstanceAuthorizationEventArgs((Models.InstanceInfo)sender);
                        RequestInstanceAuthorization?.Invoke(this, e_instance);
                        e.AccessToken = e_instance.AccessToken;
                    }));
            }
        }

        #endregion
    }
}
