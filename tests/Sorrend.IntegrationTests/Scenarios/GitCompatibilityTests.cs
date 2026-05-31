using Sorrend.IntegrationTests.Tools;
using Sorrend.IntegrationTests.Tools.Repositories;
using Sorrend.IntegrationTests.Tools.SystemUnderTest;
using Sorrend.IntegrationTests.Utilities;

namespace Sorrend.IntegrationTests.Scenarios;

public class GitCompatibilityTests(
    TestingEnvironment.Provider testingEnvironmentProvider,
    GitRepositoryFactory gitRepositoryFactory,
    HeadlessSut headlessSut)
{
    [Fact]
    public async Task GetsCommitHistory()
    {
        var ct = TestContext.Current.CancellationToken;

        var testingEnvironment = await testingEnvironmentProvider.GetAsync();
        var workingDirectoryPath = testingEnvironment.WorkingDirectoryPath;

        var repositoryRootDirectoryPath = workingDirectoryPath / Generate.DirectoryName();
        var someRepositoryFilePath = repositoryRootDirectoryPath / Generate.FileName();

        var expectedCommits = new List<CommitDescription>();

        var repository = await gitRepositoryFactory.CreateAsync(repositoryRootDirectoryPath);

        expectedCommits.Add(await repository.CommitAsync());

        await repository.CreateBranchAsync("develop");
        await repository.CreateBranchAsync("feature");

        await repository.CheckoutAsync("feature");

        await FileShim.WriteAllTextAsync(someRepositoryFilePath.ToString(), "contents", ct);
        await repository.CommitAsync();

        await repository.CheckoutAsync("develop");

        expectedCommits.Add(await repository.CommitAsync());

        expectedCommits.Add(await repository.MergeAsync("feature"));

        expectedCommits.Add(
            await repository.CommitAsync(
                x => x.WithAuthorDateTime("2012-08-05T22:17:57-07:00")));

        await repository.TagAsync("foo");
        await repository.TagAsync("bar");

        expectedCommits.Reverse();

        var actualCommits = await headlessSut.GitVersionControlSystem
            .GetFirstParentCommitsAsync(repositoryRootDirectoryPath);

        Assert.Equal(
            expectedCommits.Select(x => x.Hash),
            actualCommits.Select(x => x.Hash));

        Assert.Equal(
            new DateTimeOffset(2012, 8, 5, 22, 17, 57, TimeSpan.FromHours(-7)),
            actualCommits[0].AuthorDateTime);

        Assert.Collection(
            actualCommits[0].Tags,
            x => Assert.Equal("foo", x),
            x => Assert.Equal("bar", x));

        Assert.All(
            actualCommits.Skip(1),
            x => Assert.Empty(x.Tags));
    }
}
