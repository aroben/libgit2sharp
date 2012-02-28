﻿using System.Linq;
using LibGit2Sharp.Tests.TestHelpers;
using Xunit;

namespace LibGit2Sharp.Tests
{
    public class TreeFixture : BaseFixture
    {
        private const string sha = "581f9824ecaf824221bd36edf5430f2739a7c4f5";

        [Fact]
        public void CanCompareTwoTreeEntries()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var tree = repo.Lookup<Tree>(sha);
                TreeEntry treeEntry1 = tree["README"];
                TreeEntry treeEntry2 = tree["README"];
                treeEntry1.ShouldEqual(treeEntry2);
                (treeEntry1 == treeEntry2).ShouldBeTrue();
            }
        }

        [Fact]
        public void CanConvertEntryToBlob()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var tree = repo.Lookup<Tree>(sha);
                TreeEntry treeEntry = tree["README"];

                var blob = treeEntry.Target as Blob;
                blob.ShouldNotBeNull();
            }
        }

        [Fact]
        public void CanConvertEntryToTree()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var tree = repo.Lookup<Tree>(sha);
                TreeEntry treeEntry = tree["1"];

                var subtree = treeEntry.Target as Tree;
                subtree.ShouldNotBeNull();
            }
        }

        [Fact]
        public void CanEnumerateBlobs()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var tree = repo.Lookup<Tree>(sha);
                tree.Files.Count().ShouldEqual(3);
            }
        }

        [Fact]
        public void CanEnumerateSubTrees()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var tree = repo.Lookup<Tree>(sha);
                tree.Trees.Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void CanEnumerateTreeEntries()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var tree = repo.Lookup<Tree>(sha);
                tree.Count().ShouldEqual(tree.Count);

                Assert.Equal(new[] { "1", "README", "branch_file.txt", "new.txt" }, tree.Select(te => te.Name).ToArray());
            }
        }

        [Fact]
        public void CanGetEntryByName()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var tree = repo.Lookup<Tree>(sha);
                TreeEntry treeEntry = tree["README"];
                treeEntry.Target.Sha.ShouldEqual("a8233120f6ad708f843d861ce2b7228ec4e3dec6");
                treeEntry.Name.ShouldEqual("README");
            }
        }

        [Fact]
        public void GettingAnUknownTreeEntryReturnsNull()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var tree = repo.Lookup<Tree>(sha);
                TreeEntry treeEntry = tree["I-do-not-exist"];
                treeEntry.ShouldBeNull();
            }
        }

        [Fact]
        public void CanGetEntryCountFromTree()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var tree = repo.Lookup<Tree>(sha);
                tree.Count.ShouldEqual(4);
            }
        }

        [Fact]
        public void CanReadEntryAttributes()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var tree = repo.Lookup<Tree>(sha);
                tree["README"].Attributes.ShouldEqual(33188);
            }
        }

        [Fact]
        public void CanReadTheTreeData()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var tree = repo.Lookup<Tree>(sha);
                tree.ShouldNotBeNull();
            }
        }

        [Fact]
        public void TreeDataIsPresent()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                GitObject tree = repo.Lookup(sha);
                tree.ShouldNotBeNull();
            }
        }
    }
}
