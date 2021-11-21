using Octokit;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using VRCEventUtil.Models.Setting;

namespace VRCEventUtil.Models.Util
{
    /// <summary>
    /// 更新の確認を行うクラス
    /// </summary>
    internal class UpdateChecker
    {
        public UpdateChecker(string repoOwner, string repoName)
        {
            RepoOwner = repoOwner;
            RepoName = repoName;

            var assembly = Assembly.GetExecutingAssembly();
            var info = FileVersionInfo.GetVersionInfo(assembly.Location);
            var fileVersion = info.FileVersion;
            if (fileVersion is null)
            {
                Current = new Version(0, 0, 0);
            }
            else
            {
                Current = ParseVersion(fileVersion);
            }
        }

        public string RepoOwner { get; }
        public string RepoName { get; }
        public Version Current { get; }
        public Version Latest { get; private set; } = new Version(0, 0, 0);

        public async Task<bool> Check()
        {
            var github = new GitHubClient(new ProductHeaderValue(RepoName));
            Release release;
            try
            {
                if (SettingManager.Settings.CheckPreRelease)
                {
                    var releases = await github.Repository.Release.GetAll(RepoOwner, RepoName);
                    if (!releases.Any()) { return false; }
                    release = releases[0];
                }
                else
                {
                    release = await github.Repository.Release.GetLatest(RepoOwner, RepoName);
                }
            }
            catch (NotFoundException) { return false; }
            catch (ApiException ex)
            {
                Logger.Log(ex);
                return false;
            }
            Latest = ParseVersion(release.TagName);

            return Latest > Current;
        }

        /// <summary>
        /// バージョン文字列を*.*.*形式のバージョンとして解析します．先頭のvは無視されます．
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static Version ParseVersion(string version)
        {
            var versions = version.TrimStart('v').Split('.');

            int major;
            int minor;
            int build;

            if (versions.Length == 0)
            {
                return new Version();
            }
            major = int.Parse(versions[0]);

            if (version.Length == 1)
            {
                return new Version(major, 0, 0);
            }
            minor = int.Parse(versions[1]);

            if (version.Length == 2)
            {
                return new Version(minor, minor, 0);
            }
            build = int.Parse(versions[2]);

            return new Version(major, minor, build);
        }
    }
}
