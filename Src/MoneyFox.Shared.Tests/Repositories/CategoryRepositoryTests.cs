﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoneyFox.Shared.Interfaces;
using MoneyFox.Shared.Model;
using MoneyFox.Shared.Repositories;
using MoneyFox.Shared.Resources;
using MoneyFox.Shared.Tests.Mocks;
using Moq;
using Xunit;
using TestFoundation;
using MvvmCross.Test.Core;
using MvvmCross.Platform;
using System;

namespace MoneyFox.Shared.Tests.Repositories
{
    public class CategoryRepositoryTests : MvxIoCSupportingTest
    {
        private DateTime localDateSetting;

        public CategoryRepositoryTests()
        {
            Setup();

            // We setup the static setting classes here for the general usage in the app
            var settingsMockSetup = new Mock<ILocalSettings>();
            settingsMockSetup.SetupAllProperties();
            settingsMockSetup.Setup(x => x.AddOrUpdateValue(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Callback((string key, DateTime date) => localDateSetting = date);

            var roamSettingsMockSetup = new Mock<IRoamingSettings>();
            roamSettingsMockSetup.SetupAllProperties();

            Mvx.RegisterType(() => settingsMockSetup.Object);
            Mvx.RegisterType(() => roamSettingsMockSetup.Object);
        }

        public static IEnumerable NamePlaceholder
        {
            get
            {
                yield return new object[] {"Ausgang", "Ausgang"};
                yield return new object[] {"", Strings.NoNamePlaceholderLabel};
            }
        }

        [Theory]
        [MemberData(nameof(NamePlaceholder))]
        public void Save_InputName_CorrectNameAssigned(string inputName, string expectedResult)
        {
            var categoryDataAccessMock = new CategoryDataAccessMock();
            var repository = new CategoryRepository(categoryDataAccessMock);

            var category = new Category
            {
                Name = inputName
            };

            repository.Save(category);

            categoryDataAccessMock.CategoryTestList[0].ShouldBeSameAs(category);
            categoryDataAccessMock.CategoryTestList[0].Name.ShouldBe(expectedResult);
        }

        [Fact]
        public void CategoryRepository_Delete()
        {
            var categoryDataAccessMock = new CategoryDataAccessMock();
            var repository = new CategoryRepository(categoryDataAccessMock);

            var category = new Category
            {
                Name = "Ausgang"
            };

            repository.Save(category);

            categoryDataAccessMock.CategoryTestList[0].ShouldBeSameAs(category);

            repository.Delete(category);

            categoryDataAccessMock.CategoryTestList.Any().ShouldBeFalse();
            repository.Data.Any().ShouldBeFalse();
        }

        [Fact]
        public void CategoryRepository_AccessCache()
        {
            new CategoryRepository(new CategoryDataAccessMock()).Data.ShouldNotBeNull();
        }

        [Fact]
        public void CategoryRepository_AddMultipleToCache()
        {
            var repository = new CategoryRepository(new CategoryDataAccessMock());
            var category = new Category
            {
                Name = "Ausgang"
            };

            var secondCategory = new Category
            {
                Name = "Lebensmittel"
            };

            repository.Save(category);
            repository.Save(secondCategory);

            repository.Data.Count.ShouldBe(2);
            repository.Data[0].ShouldBeSameAs(category);
            repository.Data[1].ShouldBeSameAs(secondCategory);
        }

        [Fact]
        public void Load_CategoryDataAccess_DataInitialized()
        {
            var dataAccessSetup = new Mock<IDataAccess<Category>>();
            dataAccessSetup.Setup(x => x.LoadList(null)).Returns(new List<Category>
            {
                new Category {Id = 10},
                new Category {Id = 15}
            });

            var categoryRepository = new CategoryRepository(dataAccessSetup.Object);
            categoryRepository.Load();

            categoryRepository.Data.Any(x => x.Id == 10).ShouldBeTrue();
            categoryRepository.Data.Any(x => x.Id == 15).ShouldBeTrue();
        }

        [Fact]
        public void Save_UpdateTimeStamp()
        {
            var dataAccessSetup = new Mock<IDataAccess<Category>>();
            dataAccessSetup.Setup(x => x.LoadList(null)).Returns(new List<Category>());

            new CategoryRepository(dataAccessSetup.Object).Save(new Category());
            localDateSetting.ShouldBeInRange(DateTime.Now.AddSeconds(-1), DateTime.Now.AddSeconds(1));
        }
    }
}