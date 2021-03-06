﻿//------------------------------------------------------------------------------
// <copyright file="CmdBase.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.Commands
{
	public abstract class CmdBase
	{
		/// <summary>
		/// VS Package that provides this command, not null.
		/// </summary>
		protected readonly IDialogFactory dialogFactory;
		protected readonly IProjectManager projectManager;
		protected readonly IProjectConfigurationManager projectConfigurationManager;

		public CmdBase(
			IProjectManager projectManager,
			IDialogFactory dialogFactory, 
			IProjectConfigurationManager projectConfigurationManager)
		{
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (projectManager == null) throw new ArgumentNullException(nameof(projectManager));
			if (projectConfigurationManager == null) throw new ArgumentNullException(nameof(projectConfigurationManager));

			this.dialogFactory = dialogFactory;
			this.projectManager = projectManager;
			this.projectConfigurationManager = projectConfigurationManager;
		}

		public virtual void ExecuteCommand(object sender, EventArgs args)
		{
			try
			{
				string projectConfigPath = projectManager.ProjectConfigPath;
				var projectConfiguration = projectConfigurationManager.Load(projectConfigPath);

				projectConfiguration.Update(projectManager);
				projectConfigurationManager.Save(projectConfigPath, projectConfiguration);

				bool saved = projectManager.SaveDirtyFiles(projectConfiguration.MethodInfos);
				if (saved)
				{
					ExecuteCommandImpl(sender, args);
				}
			}
			catch (Exception ex)
			{
				var messageWindow = dialogFactory.GetMessageBoxWindow();
				messageWindow.ShowDialog(ex.Message,
					"Aras VS method plugin",
					MessageButtons.OK,
					MessageIcon.Error);
			}
		}
		//TODO: remove uiShell from parameters
		public abstract void ExecuteCommandImpl(object sender, EventArgs args);

		protected virtual void CheckCommandAccessibility(object sender, EventArgs e)
		{
			var command = (OleMenuCommand)sender;
			if (this.projectManager.SolutionHasProject && this.projectManager.IsArasProject && this.projectManager.IsCommandForMethod(command.CommandID.Guid))
			{
				command.Enabled = true;
				return;
			}

			command.Enabled = false;
		}
	}
}
