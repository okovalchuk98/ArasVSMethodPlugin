﻿using System;
using System.IO;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.Templates;
using Aras.VS.MethodPlugin.Tests.Dialogs.SubAdapters;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.ViewModels
{
	[TestFixture]
	class OpenFromPackageViewModelTest
	{
		private OpenFromPackageViewModel openFromPackageViewModel;
		private IDialogFactory dialogFactory;
		private IProjectConfiguraiton projectConfiguration;
		private TemplateLoader templateLoader;

		[SetUp]
		public void SetUp()
		{
			this.dialogFactory = Substitute.For<IDialogFactory>();
			templateLoader = new TemplateLoader(this.dialogFactory);

			this.projectConfiguration = Substitute.For<IProjectConfiguraiton>();
			this.projectConfiguration.LastSelectedSearchTypeInOpenFromPackage.Returns("MethodContent");
			openFromPackageViewModel = new OpenFromPackageViewModel(this.dialogFactory, templateLoader, "C#", this.projectConfiguration);
		}

		[Test]
		public void Ctor_ShouldDialogFactoryThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				OpenFromPackageViewModel viewModel = new OpenFromPackageViewModel(null, this.templateLoader, "C#", this.projectConfiguration);
			});
		}


		[Test]
		public void Ctor_ShouldTemplateLoaderThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				OpenFromPackageViewModel viewModel = new OpenFromPackageViewModel(this.dialogFactory, null, "C#", this.projectConfiguration);
			});
		}

		[Test]
		public void Ctor_ShouldProjectConfiguraitonThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				OpenFromPackageViewModel viewModel = new OpenFromPackageViewModel(this.dialogFactory, this.templateLoader, "C#", null);
			});
		}

		[Test]
		public void FolderBrowserCommand_ShouldLeaveNullAndEmpty()
		{
			//Arrange
			var adapter = new OpenFromPackageTreeViewAdapterTest(false, string.Empty, string.Empty, string.Empty, string.Empty);
			this.dialogFactory.GetOpenFromPackageTreeView(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(adapter);

			//Act
			this.openFromPackageViewModel.FolderBrowserCommand.Execute(null);

			//Assert
			Assert.IsNull(this.openFromPackageViewModel.MethodCode);
			Assert.IsNull(this.openFromPackageViewModel.MethodComment);
			Assert.IsNull(this.openFromPackageViewModel.IdentityId);
			Assert.IsNull(this.openFromPackageViewModel.MethodConfigId);
			Assert.IsNull(this.openFromPackageViewModel.MethodId);
			Assert.IsNull(this.openFromPackageViewModel.MethodLanguage);
			Assert.IsNull(this.openFromPackageViewModel.IdentityKeyedName);
			Assert.IsNull(this.openFromPackageViewModel.SelectedTemplate);
		}

		[Test]
		public void FolderBrowserCommand_ShouldFillExpectedProperty()
		{
			//Arrange
			string currentPath = AppDomain.CurrentDomain.BaseDirectory;
			string pathToMethodAml = Path.Combine(currentPath, @"Code\TestData\MethodAml\ReturnNullMethodAml.xml");

			var adapter = new OpenFromPackageTreeViewAdapterTest(true, "testPackageName", "MfFilePath", "searchType", pathToMethodAml);
			this.dialogFactory.GetOpenFromPackageTreeView(Arg.Is("C:\\"), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(adapter);

			//Act
			this.openFromPackageViewModel.FolderBrowserCommand.Execute(null);

			//Assert
			Assert.AreEqual("\r\nreturn null;", this.openFromPackageViewModel.MethodCode);
			Assert.IsNull(this.openFromPackageViewModel.MethodComment);
			Assert.AreEqual("A73B655731924CD0B027E4F4D5FCC0A9", this.openFromPackageViewModel.IdentityId);
			Assert.AreEqual("B4D99F186D9F3D6631927A3EB3440F99", this.openFromPackageViewModel.MethodConfigId);
			Assert.AreEqual("B4D99F186D9F3D6631927A3EB3440F99", this.openFromPackageViewModel.MethodId);
			Assert.AreEqual("C#", this.openFromPackageViewModel.MethodLanguage);
			Assert.AreEqual("World", this.openFromPackageViewModel.IdentityKeyedName);
			Assert.IsNull(this.openFromPackageViewModel.SelectedTemplate);
			Assert.AreEqual("searchType", this.openFromPackageViewModel.SelectedSearchType);
		}

		[Test]
		public void SetValueInSelectedTemplate_ShouldReceiveExpectedMessage()
		{
			//Arange
			TemplateInfo testTemplateInfo = new TemplateInfo()
			{
				Message = "Message",
				IsSuccessfullySupported = false
			};

			IMessageBoxWindow messageBoxWindow = Substitute.For<IMessageBoxWindow>();
			this.dialogFactory.GetMessageBoxWindow().Returns(messageBoxWindow);

			//Act
			this.openFromPackageViewModel.SelectedTemplate = testTemplateInfo;

			//Assert
			messageBoxWindow.Received().ShowDialog("Message",
						"Open method from AML package",
						MessageButtons.OK,
						MessageIcon.None);
		}
	}
}
