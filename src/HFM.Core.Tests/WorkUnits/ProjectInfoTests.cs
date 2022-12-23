namespace HFM.Core.WorkUnits;

[TestFixture]
public class ProjectInfoTests
{
    [TestCase(0, 0, 0, 0, ExpectedResult = false)]
    [TestCase(1, 0, 0, 0, ExpectedResult = true)]
    [TestCase(0, 1, 0, 0, ExpectedResult = true)]
    [TestCase(0, 0, 1, 0, ExpectedResult = true)]
    [TestCase(0, 0, 0, 1, ExpectedResult = true)]
    public bool HasProject(int id, int run, int clone, int gen) =>
        new ProjectInfo
        {
            ProjectId = id,
            ProjectRun = run,
            ProjectClone = clone,
            ProjectGen = gen
        }.HasProject();

    [TestCase(0, 0, 0, 0, ExpectedResult = "Project: 0 (Run 0, Clone 0, Gen 0)")]
    [TestCase(1, 0, 0, 0, ExpectedResult = "Project: 1 (Run 0, Clone 0, Gen 0)")]
    [TestCase(0, 1, 0, 0, ExpectedResult = "Project: 0 (Run 1, Clone 0, Gen 0)")]
    [TestCase(0, 0, 1, 0, ExpectedResult = "Project: 0 (Run 0, Clone 1, Gen 0)")]
    [TestCase(0, 0, 0, 1, ExpectedResult = "Project: 0 (Run 0, Clone 0, Gen 1)")]
    public string ToString(int id, int run, int clone, int gen) =>
        new ProjectInfo
        {
            ProjectId = id,
            ProjectRun = run,
            ProjectClone = clone,
            ProjectGen = gen
        }.ToString();

    [TestCase(0, 0, 0, 0, ExpectedResult = "Project: 0 (Run 0, Clone 0, Gen 0)")]
    [TestCase(1, 0, 0, 0, ExpectedResult = "Project: 1 (Run 0, Clone 0, Gen 0)")]
    [TestCase(0, 1, 0, 0, ExpectedResult = "Project: 0 (Run 1, Clone 0, Gen 0)")]
    [TestCase(0, 0, 1, 0, ExpectedResult = "Project: 0 (Run 0, Clone 1, Gen 0)")]
    [TestCase(0, 0, 0, 1, ExpectedResult = "Project: 0 (Run 0, Clone 0, Gen 1)")]
    public string ToProjectString(int id, int run, int clone, int gen) =>
        new ProjectInfo
        {
            ProjectId = id,
            ProjectRun = run,
            ProjectClone = clone,
            ProjectGen = gen
        }.ToProjectString();

    [TestCase(0, 0, 0, 0, ExpectedResult = "P0 (R0, C0, G0)")]
    [TestCase(1, 0, 0, 0, ExpectedResult = "P1 (R0, C0, G0)")]
    [TestCase(0, 1, 0, 0, ExpectedResult = "P0 (R1, C0, G0)")]
    [TestCase(0, 0, 1, 0, ExpectedResult = "P0 (R0, C1, G0)")]
    [TestCase(0, 0, 0, 1, ExpectedResult = "P0 (R0, C0, G1)")]
    public string ToShortProjectString(int id, int run, int clone, int gen) =>
        new ProjectInfo
        {
            ProjectId = id,
            ProjectRun = run,
            ProjectClone = clone,
            ProjectGen = gen
        }.ToShortProjectString();

    [TestFixture]
    public class GivenProjectInfoIsNull
    {
        [Test]
        public void ThenHasProjectReturnsFalse()
        {
            IProjectInfo? projectInfo = null;
            Assert.That(projectInfo.HasProject(), Is.False);
        }

        [Test]
        public void ThenToProjectStringReturnsEmptyString()
        {
            IProjectInfo? projectInfo = null;
            Assert.That(projectInfo.ToProjectString(), Is.Empty);
        }

        [Test]
        public void ThenToShortProjectStringReturnsEmptyString()
        {
            IProjectInfo? projectInfo = null;
            Assert.That(projectInfo.ToShortProjectString(), Is.Empty);
        }
    }

    [TestFixture]
    public class GivenTwoNullProjectInfos
    {
        [Test]
        public void ThenProjectsAreNotEqual()
        {
            IProjectInfo? projectInfo1 = null;
            IProjectInfo? projectInfo2 = null;
            Assert.That(projectInfo1.EqualsProject(projectInfo2), Is.False);
        }
    }

    [TestFixture]
    public class GivenFirstProjectInfoIsNull
    {
        [Test]
        public void ThenProjectsAreNotEqual()
        {
            IProjectInfo? projectInfo1 = null;
            var projectInfo2 = new ProjectInfo { ProjectId = 5, ProjectRun = 6, ProjectClone = 7, ProjectGen = 8 };
            Assert.That(projectInfo1.EqualsProject(projectInfo2), Is.False);
        }
    }

    [TestFixture]
    public class GivenSecondProjectInfoIsNull
    {
        [Test]
        public void ThenProjectsAreNotEqual()
        {
            var projectInfo1 = new ProjectInfo { ProjectId = 1, ProjectRun = 2, ProjectClone = 3, ProjectGen = 4 };
            IProjectInfo? projectInfo2 = null;
            Assert.That(projectInfo1.EqualsProject(projectInfo2), Is.False);
        }
    }

    [TestFixture]
    public class GivenTwoDefaultProjectInfos
    {
        [Test]
        public void ThenProjectsAreEqual()
        {
            var projectInfo1 = new ProjectInfo();
            var projectInfo2 = new ProjectInfo();
            Assert.That(projectInfo1.EqualsProject(projectInfo2), Is.True);
        }
    }

    [TestFixture]
    public class GivenTwoProjectInfosWithSameProjectRunCloneGen
    {
        [Test]
        public void ThenProjectsAreEqual()
        {
            var projectInfo1 = new ProjectInfo { ProjectId = 1, ProjectRun = 2, ProjectClone = 3, ProjectGen = 4 };
            var projectInfo2 = new ProjectInfo { ProjectId = 1, ProjectRun = 2, ProjectClone = 3, ProjectGen = 4 };
            Assert.That(projectInfo1.EqualsProject(projectInfo2), Is.True);
        }
    }

    [TestFixture]
    public class GivenTwoProjectInfosWithDifferentProjectRunCloneGen
    {
        [Test]
        public void ThenProjectsAreNotEqual()
        {
            var projectInfo1 = new ProjectInfo { ProjectId = 1, ProjectRun = 2, ProjectClone = 3, ProjectGen = 4 };
            var projectInfo2 = new ProjectInfo { ProjectId = 5, ProjectRun = 6, ProjectClone = 7, ProjectGen = 8 };
            Assert.That(projectInfo1.EqualsProject(projectInfo2), Is.False);
        }
    }
}
