using HFM.Core.Artifacts;
using HFM.Core.Logging;
using HFM.Core.ScheduledTasks;
using HFM.Core.WorkUnits;
using HFM.Preferences;
using HFM.Preferences.Data;

using Moq;

namespace HFM.Core.Client;

[TestFixture]
public class ClientScheduledTasksTests : IDisposable
{
    private IPreferences? _preferences;
    private ClientConfiguration? _configuration;
    private ClientScheduledTasks? _clientScheduledTasks;

    [SetUp]
    public virtual Task BeforeEach()
    {
        _preferences = new InMemoryPreferencesProvider();
        _configuration = new ClientConfiguration(MockClientFactory.Instance);
        _clientScheduledTasks = new ClientScheduledTasks(
            NullLogger.Instance,
            _preferences,
            _configuration,
            NullWebGenerationArtifactDeployment.Instance);

        return Task.CompletedTask;
    }

    [TestFixture]
    public class GivenClientConfiguration : ClientScheduledTasksTests
    {
        [TestFixture]
        public class WhenClientsAreLoaded : GivenClientConfiguration
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                _configuration!.Load(new[]
                {
                        new ClientSettings
                        {
                            Guid = Guid.NewGuid()
                        },
                        new ClientSettings
                        {
                            Guid = Guid.NewGuid()
                        }
                    });

                await WaitForTaskCompletion();
            }

            [Test]
            public void ThenConfigurationContainsClients() =>
                Assert.That(_configuration, Has.Count.EqualTo(2));

            [Test]
            public void ThenAddedClientsAreRefreshed()
            {
                foreach (var client in _configuration!)
                {
                    var mockClient = Mock.Get(client);
                    mockClient.Verify(x => x!.Refresh(It.IsAny<CancellationToken>()), Times.Once);
                }
            }
        }

        [TestFixture]
        public class WhenClientIsAdded : GivenClientConfiguration
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                _configuration!.Add(new ClientSettings());

                await WaitForTaskCompletion();
            }

            [Test]
            public void ThenConfigurationContainsClient() =>
                Assert.That(_configuration, Has.Count.EqualTo(1));

            [Test]
            public void ThenAddedClientIsRefreshed()
            {
                var client = _configuration!.First();
                var mockClient = Mock.Get(client);
                mockClient.Verify(x => x!.Refresh(It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [TestFixture]
        public class WhenClientIsEdited : GivenClientConfiguration
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                var guid = Guid.NewGuid();

                _configuration!.Add(new ClientSettings { Guid = guid });

                await WaitForTaskCompletion();

                _configuration!.Edit(new ClientSettings { Guid = guid, Name = "the name changed" });

                await WaitForTaskCompletion();
            }

            [Test]
            public void ThenConfigurationContainsClient() =>
                Assert.That(_configuration, Has.Count.EqualTo(1));

            [Test]
            public void ThenEditedClientIsRefreshed()
            {
                var client = _configuration!.First();
                var mockClient = Mock.Get(client);
                mockClient.Verify(x => x!.Refresh(It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [TestFixture]
        public class WhenConfigurationIsCleared : GivenClientConfiguration
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                _configuration!.Load(new[]
                {
                        new ClientSettings
                        {
                            Guid = Guid.NewGuid()
                        },
                        new ClientSettings
                        {
                            Guid = Guid.NewGuid()
                        }
                    });

                await WaitForTaskCompletion();

                _configuration!.Clear();

                await WaitForTaskCompletion();
            }

            [Test]
            public void ThenConfigurationContainsNoClients() =>
                Assert.That(_configuration, Has.Count.EqualTo(0));
        }

        [TestFixture]
        public class WhenConfigurationWithClientIsRefreshed : GivenClientConfiguration
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                _configuration!.Load(new[]
                {
                        new ClientSettings
                        {
                            Guid = Guid.NewGuid()
                        },
                        new ClientSettings
                        {
                            Guid = Guid.NewGuid()
                        }
                    });

                await WaitForTaskCompletion();

                _configuration!.Refresh();

                await WaitForTaskCompletion();
            }

            [Test]
            public void ThenAllClientsAreRefreshed()
            {
                foreach (var client in _configuration!)
                {
                    var mockClient = Mock.Get(client);
                    mockClient.Verify(x => x!.Refresh(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
                    mockClient.Verify(x => x!.Refresh(It.IsAny<CancellationToken>()), Times.AtMost(2));
                }
            }
        }
    }

    [TestFixture]
    public class GivenClientAndWebTasksAreEnabled : ClientScheduledTasksTests
    {
        [SetUp]
        public override async Task BeforeEach()
        {
            await base.BeforeEach();

            var task = _preferences!.Get<WebGenerationTask>(Preference.WebGenerationTask);
            task!.Enabled = true;
            _preferences.Set(Preference.WebGenerationTask, task);
        }

        [TestFixture]
        public class WhenClientsAreLoaded : GivenClientAndWebTasksAreEnabled
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                _configuration!.Load(new[]
                {
                    new ClientSettings
                    {
                        Guid = Guid.NewGuid()
                    },
                    new ClientSettings
                    {
                        Guid = Guid.NewGuid()
                    }
                });

                await WaitForTaskCompletion();
            }

            [Test]
            public void ThenClientRefreshTaskIsEnabled() =>
                Assert.That(_clientScheduledTasks!.ClientRefreshTask.Enabled, Is.True);

            [Test]
            public void ThenWebArtifactTaskIsEnabled() =>
                Assert.That(_clientScheduledTasks!.WebArtifactsTask.Enabled, Is.True);
        }

        [TestFixture]
        public class WhenClientIsAdded : GivenClientAndWebTasksAreEnabled
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                _configuration!.Add(new ClientSettings());

                await WaitForTaskCompletion();
            }

            [Test]
            public void ThenClientRefreshTaskIsEnabled() =>
                Assert.That(_clientScheduledTasks!.ClientRefreshTask.Enabled, Is.True);

            [Test]
            public void ThenWebArtifactTaskIsEnabled() =>
                Assert.That(_clientScheduledTasks!.WebArtifactsTask.Enabled, Is.True);
        }

        [TestFixture]
        public class WhenConfigurationIsCleared : GivenClientAndWebTasksAreEnabled
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                _configuration!.Load(new[]
                {
                    new ClientSettings
                    {
                        Guid = Guid.NewGuid()
                    },
                    new ClientSettings
                    {
                        Guid = Guid.NewGuid()
                    }
                });

                await WaitForTaskCompletion();

                _configuration!.Clear();

                await WaitForTaskCompletion();
            }

            [Test]
            public void ThenClientRefreshTaskIsNotEnabled() =>
                Assert.That(_clientScheduledTasks!.ClientRefreshTask.Enabled, Is.False);

            [Test]
            public void ThenWebArtifactTaskIsNotEnabled() =>
                Assert.That(_clientScheduledTasks!.WebArtifactsTask.Enabled, Is.False);
        }
    }

    [TestFixture]
    public class GivenClientAndWebTasksAreEnabledAndWebTaskRunsAfterClientRefresh : ClientScheduledTasksTests
    {
        [SetUp]
        public override async Task BeforeEach()
        {
            await base.BeforeEach();

            var task = _preferences!.Get<WebGenerationTask>(Preference.WebGenerationTask);
            task!.Enabled = true;
            task.AfterClientRetrieval = true;
            _preferences.Set(Preference.WebGenerationTask, task);
        }

        [TestFixture]
        public class WhenClientsAreLoaded : GivenClientAndWebTasksAreEnabledAndWebTaskRunsAfterClientRefresh
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                _configuration!.Load(new[]
                {
                    new ClientSettings
                    {
                        Guid = Guid.NewGuid()
                    },
                    new ClientSettings
                    {
                        Guid = Guid.NewGuid()
                    }
                });

                await WaitForTaskCompletion();
            }

            [Test]
            public void ThenClientRefreshTaskIsEnabled() =>
                Assert.That(_clientScheduledTasks!.ClientRefreshTask.Enabled, Is.True);

            [Test]
            public void ThenWebArtifactTaskIsNotEnabled() =>
                Assert.That(_clientScheduledTasks!.WebArtifactsTask.Enabled, Is.False);
        }

        [TestFixture]
        public class WhenClientIsAdded : GivenClientAndWebTasksAreEnabledAndWebTaskRunsAfterClientRefresh
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                _configuration!.Add(new ClientSettings());

                await WaitForTaskCompletion();
            }

            [Test]
            public void ThenClientRefreshTaskIsEnabled() =>
                Assert.That(_clientScheduledTasks!.ClientRefreshTask.Enabled, Is.True);

            [Test]
            public void ThenWebArtifactTaskIsNotEnabled() =>
                Assert.That(_clientScheduledTasks!.WebArtifactsTask.Enabled, Is.False);
        }

        [TestFixture]
        public class WhenConfigurationIsCleared : GivenClientAndWebTasksAreEnabledAndWebTaskRunsAfterClientRefresh
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                _configuration!.Load(new[]
                {
                    new ClientSettings
                    {
                        Guid = Guid.NewGuid()
                    },
                    new ClientSettings
                    {
                        Guid = Guid.NewGuid()
                    }
                });

                await WaitForTaskCompletion();

                _configuration!.Clear();

                await WaitForTaskCompletion();
            }

            [Test]
            public void ThenClientRefreshTaskIsNotEnabled() =>
                Assert.That(_clientScheduledTasks!.ClientRefreshTask.Enabled, Is.False);

            [Test]
            public void ThenWebArtifactTaskIsNotEnabled() =>
                Assert.That(_clientScheduledTasks!.WebArtifactsTask.Enabled, Is.False);
        }
    }

    [TestFixture]
    public class GivenClientAndWebTasksAreNotEnabled : ClientScheduledTasksTests
    {
        [SetUp]
        public override async Task BeforeEach()
        {
            await base.BeforeEach();

            var client = _preferences!.Get<ClientRetrievalTask>(Preference.ClientRetrievalTask);
            client!.Enabled = false;
            _preferences.Set(Preference.ClientRetrievalTask, client);

            var web = _preferences!.Get<WebGenerationTask>(Preference.WebGenerationTask);
            web!.Enabled = false;
            _preferences.Set(Preference.WebGenerationTask, web);

            _configuration!.Load(new[]
            {
                new ClientSettings
                {
                    Guid = Guid.NewGuid()
                },
                new ClientSettings
                {
                    Guid = Guid.NewGuid()
                }
            });

            await WaitForTaskCompletion();
        }

        [TestFixture]
        public class WhenClientRefreshTaskEnabledChanged : GivenClientAndWebTasksAreNotEnabled
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                var task = _preferences!.Get<ClientRetrievalTask>(Preference.ClientRetrievalTask);
                task!.Enabled = true;
                _preferences.Set(Preference.ClientRetrievalTask, task);
            }

            [Test]
            public void ThenClientRefreshTaskIsEnabled() =>
                Assert.That(_clientScheduledTasks!.ClientRefreshTask.Enabled, Is.True);
        }

        [TestFixture]
        public class WhenWebArtifactsTaskEnabledChanged : GivenClientAndWebTasksAreNotEnabled
        {
            [SetUp]
            public override async Task BeforeEach()
            {
                await base.BeforeEach();

                var task = _preferences!.Get<WebGenerationTask>(Preference.WebGenerationTask);
                task!.Enabled = true;
                _preferences.Set(Preference.WebGenerationTask, task);
            }

            [Test]
            public void ThenWebArtifactsTaskIsEnabled() =>
                Assert.That(_clientScheduledTasks!.WebArtifactsTask.Enabled, Is.True);
        }
    }

    [TestFixture]
    public class GivenClientRefreshTaskEnabledChanged : ClientScheduledTasksTests
    {
        [SetUp]
        public override async Task BeforeEach()
        {
            await base.BeforeEach();

            _configuration!.Load(new[]
            {
                new ClientSettings
                {
                    Guid = Guid.NewGuid()
                },
                new ClientSettings
                {
                    Guid = Guid.NewGuid()
                }
            });

            await WaitForTaskCompletion();

            var task = _preferences!.Get<ClientRetrievalTask>(Preference.ClientRetrievalTask);
            task!.Enabled = false;
            _preferences.Set(Preference.ClientRetrievalTask, task);
        }

        [Test]
        public void ThenClientRefreshTaskIsNotEnabled() =>
            Assert.That(_clientScheduledTasks!.ClientRefreshTask.Enabled, Is.False);
    }

    [TestFixture]
    public class GivenWebArtifactsTaskEnabledChanged : ClientScheduledTasksTests
    {
        [SetUp]
        public override async Task BeforeEach()
        {
            await base.BeforeEach();

            var task = _preferences!.Get<WebGenerationTask>(Preference.WebGenerationTask);
            task!.Enabled = true;
            _preferences.Set(Preference.WebGenerationTask, task);

            _configuration!.Load(new[]
            {
                new ClientSettings
                {
                    Guid = Guid.NewGuid()
                },
                new ClientSettings
                {
                    Guid = Guid.NewGuid()
                }
            });

            await WaitForTaskCompletion();

            task = _preferences!.Get<WebGenerationTask>(Preference.WebGenerationTask);
            task!.Enabled = false;
            _preferences.Set(Preference.WebGenerationTask, task);
        }

        [Test]
        public void ThenWebArtifactTaskIsNotEnabled() =>
            Assert.That(_clientScheduledTasks!.WebArtifactsTask.Enabled, Is.False);
    }

    [TestFixture]
    public class GivenPpdCalculationPreferenceIsChanged : ClientScheduledTasksTests
    {
        [SetUp]
        public override async Task BeforeEach()
        {
            await base.BeforeEach();

            _configuration!.Load(new[]
            {
                new ClientSettings
                {
                    Guid = Guid.NewGuid()
                },
                new ClientSettings
                {
                    Guid = Guid.NewGuid()
                }
            });

            await WaitForTaskCompletion();

            _preferences!.Set(Preference.PPDCalculation, PpdCalculation.AllFrames);
        }

        [Test]
        public void ThenAllClientsAreRefreshed()
        {
            foreach (var client in _configuration!)
            {
                var mockClient = Mock.Get(client);
                mockClient.Verify(x => x!.Refresh(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
                mockClient.Verify(x => x!.Refresh(It.IsAny<CancellationToken>()), Times.AtMost(2));
            }
        }
    }

    private async Task WaitForTaskCompletion()
    {
        await WaitForTaskCompletion(_clientScheduledTasks!.ClientRefreshTask);
        await WaitForTaskCompletion(_clientScheduledTasks!.WebArtifactsTask);
    }

    private static async Task WaitForTaskCompletion(IScheduledTaskInfo task)
    {
        while (task.InProgress)
        {
            TestLogger.Instance.Info("waiting for task completion");
            await Task.Delay(100);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _clientScheduledTasks?.Dispose();
    }

    private sealed class MockClientFactory : IClientFactory
    {
        public static MockClientFactory Instance { get; } = new();

        public IClient? Create(ClientSettings settings)
        {
            var mockClient = new Mock<IClient>();
            mockClient.Setup(x => x.Settings).Returns(settings);
            return mockClient.Object;
        }
    }
}
