using System;
using System.Collections.Generic;
using MoneyFox.Droid.Fragments;
using MoneyFox.Shared.ViewModels;
using MvvmCross.Droid.Shared.Caching;

namespace MoneyFox.Droid.Activities.Caching
{
    internal class MainActivityFragmentCacheInfoFactory : MvxCachedFragmentInfoFactory
    {
        private static readonly Dictionary<string, MainActivity.CustomFragmentInfo> MyFragmentsInfo = new Dictionary
            <string, MainActivity.CustomFragmentInfo>
        {
            {
                typeof(MenuViewModel).ToString(),
                new MainActivity.CustomFragmentInfo(typeof(MenuViewModel).Name,
                    typeof(MenuFragment),
                    typeof(MenuViewModel))
            },
            {
                typeof(AccountListViewModel).ToString(),
                new MainActivity.CustomFragmentInfo(typeof(AccountListViewModel).Name,
                    typeof(AccountListFragment),
                    typeof(AccountListViewModel), isRoot: true)
            },
            {
                typeof(BalanceViewModel).ToString(),
                new MainActivity.CustomFragmentInfo(typeof(BalanceViewModel).Name,
                    typeof(BalanceFragment),
                    typeof(BalanceViewModel), isRoot: true)
            },
            {
                typeof(StatisticViewModel).ToString(),
                new MainActivity.CustomFragmentInfo(typeof(StatisticViewModel).Name,
                    typeof(StatisticSelectorFragment),
                    typeof(StatisticViewModel), isRoot: true)
            },
            {
                typeof(BackupViewModel).ToString(),
                new MainActivity.CustomFragmentInfo(typeof(BackupViewModel).Name,
                    typeof(BackupFragment),
                    typeof(BackupViewModel), isRoot: true)
            },
            {
                typeof(SettingsViewModel).ToString(),
                new MainActivity.CustomFragmentInfo(typeof(SettingsViewModel).Name,
                    typeof(SettingsFragment),
                    typeof(SettingsViewModel), isRoot: true)
            },
            {
                typeof(AboutViewModel).ToString(),
                new MainActivity.CustomFragmentInfo(typeof(AboutViewModel).Name,
                    typeof(AboutFragment),
                    typeof(AboutViewModel), isRoot: true)
            },
            {
                typeof(StatisticSelectorViewModel).ToString(),
                new MainActivity.CustomFragmentInfo(typeof(StatisticSelectorViewModel).Name,
                    typeof(StatisticSelectorFragment),
                    typeof(StatisticSelectorViewModel), isRoot: true)
            }
        };

        public Dictionary<string, MainActivity.CustomFragmentInfo> GetFragmentsRegistrationData() => MyFragmentsInfo;

        public override IMvxCachedFragmentInfo CreateFragmentInfo(string tag, Type fragmentType, Type viewModelType,
            bool cacheFragment = true,
            bool addToBackstack = false)
        {
            var viewModelTypeString = viewModelType.ToString();
            if (!MyFragmentsInfo.ContainsKey(viewModelTypeString))
            {
                return base.CreateFragmentInfo(tag, fragmentType, viewModelType, cacheFragment, true);
            }

            var fragInfo = MyFragmentsInfo[viewModelTypeString];
            return fragInfo;
        }

        public override SerializableMvxCachedFragmentInfo GetSerializableFragmentInfo(
            IMvxCachedFragmentInfo objectToSerialize)
        {
            var baseSerializableCachedFragmentInfo = base.GetSerializableFragmentInfo(objectToSerialize);
            var customFragmentInfo = objectToSerialize as MainActivity.CustomFragmentInfo;

            return new SerializableCustomFragmentInfo(baseSerializableCachedFragmentInfo)
            {
                IsRoot = customFragmentInfo?.IsRoot ?? false
            };
        }

        public override IMvxCachedFragmentInfo ConvertSerializableFragmentInfo(
            SerializableMvxCachedFragmentInfo fromSerializableMvxCachedFragmentInfo)
        {
            var serializableCustomFragmentInfo = fromSerializableMvxCachedFragmentInfo as SerializableCustomFragmentInfo;
            var baseCachedFragmentInfo = base.ConvertSerializableFragmentInfo(fromSerializableMvxCachedFragmentInfo);

            return new MainActivity.CustomFragmentInfo(baseCachedFragmentInfo.Tag, baseCachedFragmentInfo.FragmentType,
                baseCachedFragmentInfo.ViewModelType, baseCachedFragmentInfo.AddToBackStack,
                serializableCustomFragmentInfo?.IsRoot ?? false)
            {
                ContentId = baseCachedFragmentInfo.ContentId,
                CachedFragment = baseCachedFragmentInfo.CachedFragment
            };
        }

        internal class SerializableCustomFragmentInfo : SerializableMvxCachedFragmentInfo
        {
            public SerializableCustomFragmentInfo()
            {
            }

            public SerializableCustomFragmentInfo(SerializableMvxCachedFragmentInfo baseFragmentInfo)
            {
                AddToBackStack = baseFragmentInfo.AddToBackStack;
                ContentId = baseFragmentInfo.ContentId;
                FragmentType = baseFragmentInfo.FragmentType;
                Tag = baseFragmentInfo.Tag;
                ViewModelType = baseFragmentInfo.ViewModelType;
                CacheFragment = baseFragmentInfo.CacheFragment;
            }

            public bool IsRoot { get; set; }
        }
    }
}