﻿//------------------------------------------------------------------------------
// <copyright file="PackageMethodInfo.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace Aras.VS.MethodPlugin.Configurations.ProjectConfigurations
{
	public class PackageMethodInfo : MethodInfo
	{
		public PackageMethodInfo()
		{

		}

		public PackageMethodInfo(PackageMethodInfo packageMethodInfo) : base(packageMethodInfo)
		{
			this.ManifestFileName = packageMethodInfo.ManifestFileName;
		}

		public string ManifestFileName{ get; set; }
	}
}
