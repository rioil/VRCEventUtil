﻿using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace VRCEventUtil.Models.Util
{
    internal class UpdateChecker
    {
        public UpdateChecker()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var info = FileVersionInfo.GetVersionInfo(assembly.Location);
            Current = ParseVersion(info.FileVersion);
        }

        public Version Current { get; }
        public Version Latest { get; private set; } = new Version();

        public async Task<bool> Check()
        {
            var github = new GitHubClient(new ProductHeaderValue("VRCEventUtil"));
            Release release;
            try
            {
                release = await github.Repository.Release.GetLatest("rioil", "VRCEventUtil");
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

        private static Version ParseVersion(string version)
        {
            var versions = version.TrimStart('v').Split('.');

            int major;
            int minor;
            int build;
            int revision;

            if (versions.Length == 0)
            {
                return new Version();
            }
            major = int.Parse(versions[0]);

            if (version.Length == 1)
            {
                return new Version(major, 0);
            }
            minor = int.Parse(versions[1]);

            if (version.Length == 2)
            {
                return new Version(minor, minor);
            }
            build = int.Parse(versions[2]);

            if (version.Length == 3)
            {
                return new Version(major, minor, build);
            }

            revision = int.Parse(versions[3]);
            return new Version(major, minor, build, revision);
        }
    }
}
