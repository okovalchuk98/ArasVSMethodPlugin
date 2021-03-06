﻿//------------------------------------------------------------------------------
// <copyright file="PartialCodeInfo.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.VS.MethodPlugin.Code
{
	public class PartialCodeInfo
	{
		public string MethodName { get; set; }

		public string Namespace { get; set; }

		public string Path { get; set; }

		public string ParentClassName { get; set; }

		public string SourceCode { get; set; }
	}
}
