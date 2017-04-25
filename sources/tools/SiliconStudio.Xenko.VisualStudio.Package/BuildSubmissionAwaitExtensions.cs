// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Threading.Tasks;
using Microsoft.Build.Execution;

// Source: http://blog.nerdbank.net/2011/07/c-await-for-msbuild.html

namespace SiliconStudio.Xenko.VisualStudio
{
    public static class BuildSubmissionAwaitExtensions
    {
        public static Task<BuildResult> ExecuteAsync(this BuildSubmission submission)
        {
            var tcs = new TaskCompletionSource<BuildResult>();
            submission.ExecuteAsync(SetBuildComplete, tcs);
            return tcs.Task;
        }

        private static void SetBuildComplete(BuildSubmission submission)
        {
            var tcs = (TaskCompletionSource<BuildResult>)submission.AsyncContext;
            tcs.SetResult(submission.BuildResult);
        }
    }
}
