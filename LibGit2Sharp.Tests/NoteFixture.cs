﻿using System;
using System.Linq;
using LibGit2Sharp.Core;
using LibGit2Sharp.Core.Compat;
using LibGit2Sharp.Tests.TestHelpers;
using Xunit;

namespace LibGit2Sharp.Tests
{
    public class NoteFixture : BaseFixture
    {
        private static readonly Signature signatureNullToken = new Signature("nulltoken", "emeric.fermas@gmail.com", DateTimeOffset.UtcNow);
        private static readonly Signature signatureYorah = new Signature("yorah", "yoram.harmelin@gmail.com", Epoch.ToDateTimeOffset(1300557894, 60));

        [Fact]
        public void RetrievingNotesFromANonExistingGitObjectYieldsNoResult()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var notes = repo.Notes[ObjectId.Zero];

                Assert.Equal(0, notes.Count());
            }
        }

        [Fact]
        public void RetrievingNotesFromAGitObjectWhichHasNoNoteYieldsNoResult()
        {
            using (var repo = new Repository(BareTestRepoPath))
            {
                var notes = repo.Notes[new ObjectId("4c062a6361ae6959e06292c1fa5e2822d9c96345")];

                Assert.Equal(0, notes.Count());
            }
        }

        /*
         * $ git show 4a202 --show-notes=*
         * commit 4a202b346bb0fb0db7eff3cffeb3c70babbd2045
         * Author: Scott Chacon <schacon@gmail.com>
         * Date:   Mon May 24 10:19:04 2010 -0700
         *
         *     a third commit
         *
         * Notes:
         *     Just Note, don't you understand?
         *
         * Notes (answer):
         *     Nope
         *
         * Notes (answer2):
         *     Not Nope, Note!
         */
        [Fact]
        public void CanRetrieveNotesFromAGitObject()
        {
            var expectedMessages = new [] { "Just Note, don't you understand?\n", "Nope\n", "Not Nope, Note!\n" };

            using (var repo = new Repository(BareTestRepoPath))
            {
                var notes = repo.Notes[new ObjectId("4a202b346bb0fb0db7eff3cffeb3c70babbd2045")];

                Assert.NotNull(notes);
                Assert.Equal(3, notes.Count());
                Assert.Equal(expectedMessages, notes.Select(n => n.Message));
            }
        }

        [Fact]
        public void CanGetListOfNotesNamespaces()
        {
            var expectedNamespaces = new[] { "commits", "answer", "answer2" };

            using (var repo = new Repository(BareTestRepoPath))
            {
                Assert.Equal(expectedNamespaces, repo.Notes.Namespaces);
                Assert.Equal(repo.Notes.DefaultNamespace, repo.Notes.Namespaces.First());
            }
        }

        /*
         * $ git show 4a202b346bb0fb0db7eff3cffeb3c70babbd2045 --show-notes=*
         * commit 4a202b346bb0fb0db7eff3cffeb3c70babbd2045
         * Author: Scott Chacon <schacon@gmail.com>
         * Date:   Mon May 24 10:19:04 2010 -0700
         *
         *     a third commit
         *
         * Notes:
         *     Just Note, don't you understand?
         *
         * Notes (answer):
         *     Nope
         *
         * Notes (answer2):
         *     Not Nope, Note!
         */
        [Fact]
        public void CanAccessNotesFromACommit()
        {
            var expectedNamespaces = new[] { "Just Note, don't you understand?\n", "Nope\n", "Not Nope, Note!\n" };

            TemporaryCloneOfTestRepo path = BuildTemporaryCloneOfTestRepo();
            using (var repo = new Repository(path.RepositoryPath))
            {
                var commit = repo.Lookup<Commit>("4a202b346bb0fb0db7eff3cffeb3c70babbd2045");

                Assert.Equal(expectedNamespaces, commit.Notes.Select(n => n.Message));

                // Make sure that Commit.Notes is not refreshed automatically
                repo.Notes.Create(commit.Id, "I'm batman!\n", signatureNullToken, signatureYorah, "batmobile");

                Assert.Equal(expectedNamespaces, commit.Notes.Select(n => n.Message));
            }
        }

        [Fact]
        public void CanCreateANoteOnAGitObject()
        {
            TemporaryCloneOfTestRepo path = BuildTemporaryCloneOfTestRepo();
            using (var repo = new Repository(path.RepositoryPath))
            {
                var commit = repo.Lookup<Commit>("9fd738e8f7967c078dceed8190330fc8648ee56a");
                var note = repo.Notes.Create(commit.Id, "I'm batman!\n", signatureNullToken, signatureYorah, "batmobile");

                var newNote = commit.Notes.Single();
                Assert.Equal(note, newNote);

                Assert.Equal("I'm batman!\n", newNote.Message);
                Assert.Equal("batmobile", newNote.Namespace);
            }
        }

        [Fact]
        public void CreatingANoteWhichAlreadyExistsOverwritesThePreviousNote()
        {
            TemporaryCloneOfTestRepo path = BuildTemporaryCloneOfTestRepo();
            using (var repo = new Repository(path.RepositoryPath))
            {
                var commit = repo.Lookup<Commit>("5b5b025afb0b4c913b4c338a42934a3863bf3644");
                Assert.NotNull(commit.Notes.FirstOrDefault(x => x.Namespace == "answer"));

                repo.Notes.Create(commit.Id, "I'm batman!\n", signatureNullToken, signatureYorah, "answer");
                var note = repo.Notes[new ObjectId("5b5b025afb0b4c913b4c338a42934a3863bf3644")].FirstOrDefault(x => x.Namespace == "answer");

                Assert.NotNull(note);

                Assert.Equal("I'm batman!\n", note.Message);
                Assert.Equal("answer", note.Namespace);
            }
        }

        [Fact]
        public void CanCompareTwoUniqueNotes()
        {
            TemporaryCloneOfTestRepo path = BuildTemporaryCloneOfTestRepo();
            using (var repo = new Repository(path.RepositoryPath))
            {
                var commit = repo.Lookup<Commit>("9fd738e8f7967c078dceed8190330fc8648ee56a");

                var firstNote = repo.Notes.Create(commit.Id, "I'm batman!\n", signatureNullToken, signatureYorah, "batmobile");
                var secondNote = repo.Notes.Create(commit.Id, "I'm batman!\n", signatureNullToken, signatureYorah, "batmobile");
                Assert.Equal(firstNote, secondNote);

                var firstNoteWithAnotherNamespace = repo.Notes.Create(commit.Id, "I'm batman!\n", signatureNullToken, signatureYorah, "batmobile2");
                Assert.NotEqual(firstNote, firstNoteWithAnotherNamespace);

                var firstNoteWithAnotherMessage = repo.Notes.Create(commit.Id, "I'm ironman!\n", signatureNullToken, signatureYorah, "batmobile");
                Assert.NotEqual(firstNote, firstNoteWithAnotherMessage);

                var anotherCommit = repo.Lookup<Commit>("c47800c7266a2be04c571c04d5a6614691ea99bd");
                var firstNoteOnAnotherCommit = repo.Notes.Create(anotherCommit.Id, "I'm batman!\n", signatureNullToken, signatureYorah, "batmobile");
                Assert.NotEqual(firstNote, firstNoteOnAnotherCommit);
            }
        }

        /*
         * $ git log 8496071c1b46c854b31185ea97743be6a8774479
         * commit 8496071c1b46c854b31185ea97743be6a8774479
         * Author: Scott Chacon <schacon@gmail.com>
         * Date:   Sat May 8 16:13:06 2010 -0700
         *
         *     testing
         *
         * Notes:
         *     Hi, I'm Note.
         */
        [Fact]
        public void CanRemoveANoteFromAGitObject()
        {
            TemporaryCloneOfTestRepo path = BuildTemporaryCloneOfTestRepo();
            using (var repo = new Repository(path.RepositoryPath))
            {
                var commit = repo.Lookup<Commit>("8496071c1b46c854b31185ea97743be6a8774479");
                var notes = repo.Notes[commit.Id];

                Assert.NotEmpty(notes);

                repo.Notes.Delete(commit.Id, signatureNullToken, signatureYorah, repo.Notes.DefaultNamespace);

                Assert.Empty(notes);
            }
        }

        /*
         * $ git show 5b5b025afb0b4c913b4c338a42934a3863bf3644 --notes=answer
         * commit 5b5b025afb0b4c913b4c338a42934a3863bf3644
         * Author: Scott Chacon <schacon@gmail.com>
         * Date:   Tue May 11 13:38:42 2010 -0700
         *
         *     another commit
         *
         * Notes (answer):
         *     Not what?
         */
        [Fact]
        public void RemovingANonExistingNoteDoesntThrow()
        {
            TemporaryCloneOfTestRepo path = BuildTemporaryCloneOfTestRepo();
            using (var repo = new Repository(path.RepositoryPath))
            {
                var commit = repo.Lookup<Commit>("5b5b025afb0b4c913b4c338a42934a3863bf3644");

                repo.Notes.Delete(commit.Id, signatureNullToken, signatureYorah, "answer2");
            }
        }

        [Fact]
        public void CanRetrieveTheListOfNotesForAGivenNamespace()
        {
            var expectedNotes = new[] { new Tuple<string, string>("1a550e416326cdb4a8e127a04dd69d7a01b11cf4", "4a202b346bb0fb0db7eff3cffeb3c70babbd2045"),
                new Tuple<string, string>("272a41cf2b22e57f2bc5bf6ef37b63568cd837e4", "8496071c1b46c854b31185ea97743be6a8774479") };

            using (var repo = new Repository(BareTestRepoPath))
            {
                Assert.Equal(expectedNotes, repo.Notes["commits"].Select(n => new Tuple<string, string>(n.BlobId.Sha, n.TargetObjectId.Sha)).ToArray());

                Assert.Equal("commits", repo.Notes.DefaultNamespace);
                Assert.Equal(expectedNotes, repo.Notes.Select(n => new Tuple<string, string>(n.BlobId.Sha, n.TargetObjectId.Sha)).ToArray());
            }
        }
    }
}
