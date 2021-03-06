﻿//------------------------------------------------------------------------------
// <copyright file="OpenFromPackageCmd.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Linq;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class OpenFromPackageCmd : CmdBase
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x102;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = CommandIds.OpenFromPackage;


		private readonly IAuthenticationManager authManager;
		private readonly ICodeProviderFactory codeProviderFactory;

		private OpenFromPackageCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory)
			: base(projectManager, dialogFactory, projectConfigurationManager)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (projectConfigurationManager == null) throw new ArgumentNullException(nameof(projectConfigurationManager));
			if (codeProviderFactory == null) throw new ArgumentNullException(nameof(codeProviderFactory));

			this.authManager = authManager;
			this.codeProviderFactory = codeProviderFactory;

			if (projectManager.CommandService != null)
			{
				var menuCommandID = new CommandID(CommandSet, CommandId);
				var menuItem = new OleMenuCommand(this.ExecuteCommand, menuCommandID);
				menuItem.BeforeQueryStatus += CheckCommandAccessibility;

				projectManager.CommandService.AddCommand(menuItem);
			}
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static OpenFromPackageCmd Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory)
		{
			Instance = new OpenFromPackageCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
		}


		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			var project = projectManager.SelectedProject;

			string projectConfigPath = projectManager.ProjectConfigPath;
			string methodConfigPath = projectManager.MethodConfigPath;

			var projectConfiguration = projectConfigurationManager.Load(projectConfigPath);
			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(project.CodeModel.Language, projectConfiguration);

			var templateLoader = new TemplateLoader(this.dialogFactory);
			templateLoader.Load(methodConfigPath);

			var openView = dialogFactory.GetOpenFromPackageView(templateLoader, codeProvider.Language, projectConfiguration);

			var openViewResult = openView.ShowDialog();
			if (openViewResult?.DialogOperationResult != true)
			{
				return;
			}

			MethodInfo methodInformation = projectConfiguration.MethodInfos.FirstOrDefault(m => m.MethodName == openViewResult.MethodName);
			bool isMethodExist = projectManager.IsMethodExist(openViewResult.MethodName);
			if (projectManager.IsMethodExist(openViewResult.MethodName))
			{
				var messageWindow = this.dialogFactory.GetMessageBoxWindow();
				var dialogReuslt = messageWindow.ShowDialog("Method already added to project. Do you want replace method?",
					"Warning",
					MessageButtons.YesNo,
					MessageIcon.None);

				if (dialogReuslt == MessageDialogResult.Yes)
				{
					projectManager.RemoveMethod(methodInformation);
					projectConfiguration.MethodInfos.Remove(methodInformation);
				}
				else
				{
					return;
				}
			}

			GeneratedCodeInfo codeInfo = codeProvider.GenerateCodeInfo(openViewResult.SelectedTemplate, openViewResult.SelectedEventSpecificData, openViewResult.MethodName, false, openViewResult.MethodCode, openViewResult.IsUseVSFormattingCode);
			projectManager.CreateMethodTree(codeInfo);

			var methodInfo = new PackageMethodInfo()
			{
				InnovatorMethodConfigId = openViewResult.MethodConfigId,
				InnovatorMethodId = openViewResult.MethodId,
				MethodLanguage = openViewResult.MethodLanguage,
				MethodName = openViewResult.MethodName,
				MethodType = openViewResult.MethodType,
				MethodComment = openViewResult.MethodComment,
				PackageName = openViewResult.Package,
				TemplateName = openViewResult.SelectedTemplate.TemplateName,
				EventData = openViewResult.SelectedEventSpecificData.EventSpecificData,
				ExecutionAllowedToId = openViewResult.IdentityId,
				ExecutionAllowedToKeyedName = openViewResult.IdentityKeyedName,
				PartialClasses = codeInfo.PartialCodeInfoList.Select(pci => pci.Path).ToList(),
				ExternalItems = codeInfo.ExternalItemsInfoList.Select(pci => pci.Path).ToList(),
				ManifestFileName = openViewResult.SelectedManifestFileName
			};

			projectConfiguration.LastSelectedDir = openViewResult.SelectedFolderPath;
			projectConfiguration.LastSelectedMfFile = openViewResult.SelectedManifestFullPath;
			projectConfiguration.UseVSFormatting = openViewResult.IsUseVSFormattingCode;
			projectConfiguration.LastSelectedSearchTypeInOpenFromPackage = openViewResult.SelectedSearchType;
			projectConfiguration.AddMethodInfo(methodInfo);
			projectConfigurationManager.Save(projectConfigPath, projectConfiguration);
		}
	}
}
