using System.ComponentModel;

namespace HFM.Core.Client;

[TestFixture]
public class ClientResourceSortComparerTests
{
    private ClientResourceSortComparer? _comparer;

    [TestFixture]
    public class GivenTwoOrMoreClientResources : ClientResourceSortComparerTests
    {
        private List<ClientResource>? _resources;

        [TestFixture]
        public class WhenFirstResourceIsOffline : GivenTwoOrMoreClientResources
        {
            [SetUp]
            public virtual void BeforeEach()
            {
                _comparer = new() { OfflineStatusLast = true };
                var property = TypeDescriptor.GetProperties(typeof(ClientResource))
                    .OfType<PropertyDescriptor>()
                    .FirstOrDefault(x => x.Name == nameof(ClientResource.Name));
                _comparer.SetSortProperties(property!, ListSortDirection.Ascending);

                _resources = new()
                {
                    new() { Status = ClientResourceStatus.Offline },
                    new() { Status = ClientResourceStatus.Running }
                };
            }

            [Test]
            public void ThenTheOfflineResourceIsLast()
            {
                var sorted = _resources!.OrderBy(x => x, _comparer).ToList();
                Assert.That(sorted.Last().Status, Is.EqualTo(ClientResourceStatus.Offline));
            }
        }

        [TestFixture]
        public class WhenSecondResourceIsOffline : GivenTwoOrMoreClientResources
        {
            [SetUp]
            public virtual void BeforeEach()
            {
                _comparer = new() { OfflineStatusLast = true };
                var property = TypeDescriptor.GetProperties(typeof(ClientResource))
                    .OfType<PropertyDescriptor>()
                    .FirstOrDefault(x => x.Name == nameof(ClientResource.Name));
                _comparer.SetSortProperties(property!, ListSortDirection.Ascending);

                _resources = new()
                {
                    new() { Status = ClientResourceStatus.Running },
                    new() { Status = ClientResourceStatus.Offline },
                    new() { Status = ClientResourceStatus.Running }
                };
            }

            [Test]
            public void ThenTheOfflineResourceIsLast()
            {
                var sorted = _resources!.OrderBy(x => x, _comparer).ToList();
                Assert.That(sorted.Last().Status, Is.EqualTo(ClientResourceStatus.Offline));
            }
        }

        [TestFixture]
        public class WhenAllResourceAreRunning : GivenTwoOrMoreClientResources
        {
            [SetUp]
            public virtual void BeforeEach()
            {
                _comparer = new() { OfflineStatusLast = true };
                var property = TypeDescriptor.GetProperties(typeof(ClientResource))
                    .OfType<PropertyDescriptor>()
                    .FirstOrDefault(x => x.Name == nameof(ClientResource.Name));
                _comparer.SetSortProperties(property!, ListSortDirection.Ascending);

                _resources = new()
                {
                    new()
                    {
                        Status = ClientResourceStatus.Running,
                        ClientIdentifier = new("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty)
                    },
                    new()
                    {
                        Status = ClientResourceStatus.Running,
                        ClientIdentifier = new("Bar", "Foo", ClientSettings.DefaultPort, Guid.Empty)
                    }
                };
            }

            [Test]
            public void ThenResourcesAreSortedByName()
            {
                var sorted = _resources!.OrderBy(x => x, _comparer).ToList();
                Assert.Multiple(() =>
                {
                    Assert.That(sorted.First().Name, Is.EqualTo("Bar"));
                    Assert.That(sorted.Last().Name, Is.EqualTo("Foo"));
                });
            }
        }

        [TestFixture]
        public class WhenSortPropertyValuesAreEqual : GivenTwoOrMoreClientResources
        {
            [SetUp]
            public virtual void BeforeEach()
            {
                _comparer = new() { OfflineStatusLast = true };
                var property = TypeDescriptor.GetProperties(typeof(ClientResource))
                    .OfType<PropertyDescriptor>()
                    .FirstOrDefault(x => x.Name == nameof(ClientResource.Core));
                _comparer.SetSortProperties(property!, ListSortDirection.Ascending);

                _resources = new()
                {
                    new()
                    {
                        Status = ClientResourceStatus.Running,
                        ClientIdentifier = new("Foo", "Bar", ClientSettings.DefaultPort, Guid.Empty),
                        WorkUnit = new()
                        {
                            Protein = new()
                            {
                                Core = "0x22"
                            }
                        }
                    },
                    new()
                    {
                        Status = ClientResourceStatus.Running,
                        ClientIdentifier = new("Bar", "Foo", ClientSettings.DefaultPort, Guid.Empty),
                        WorkUnit = new()
                        {
                            Protein = new()
                            {
                                Core = "0x22"
                            }
                        }
                    }
                };
            }

            [Test]
            public void ThenResourcesAreSortedByName()
            {
                var sorted = _resources!.OrderBy(x => x, _comparer).ToList();
                Assert.Multiple(() =>
                {
                    Assert.That(sorted.First().Name, Is.EqualTo("Bar"));
                    Assert.That(sorted.Last().Name, Is.EqualTo("Foo"));
                });
            }
        }
    }
}
