﻿using System;
using Windows.ApplicationModel.Background;
using MoneyFox.Shared;
using MoneyFox.Shared.DataAccess;
using MoneyFox.Shared.Manager;
using MoneyFox.Shared.Repositories;
using MoneyManager.Windows.Shortcut;
using MvvmCross.Plugins.File.WindowsCommon;
using MvvmCross.Plugins.Sqlite.WindowsUWP;
using Xamarin;
using MoneyManager.Windows.Services;

namespace MoneyFox.Tasks
{
    public sealed class ClearPaymentBackgroundTask : IBackgroundTask
    {
        private readonly PaymentManager paymentManager;

        public ClearPaymentBackgroundTask()
        {
            var insightKey = "599ff6bfdc79368ff3d5f5629a57c995fe93352e";

#if DEBUG
            insightKey = Insights.DebugModeKey;
#endif
            if (!Insights.IsInitialized)
            {
                Insights.Initialize(insightKey);
            }

            var sqliteConnectionCreator = new DatabaseManager(new WindowsSqliteConnectionFactory(), new MvxWindowsCommonFileStore());
            var notificationService = new NotificationService();

            var accountRepository = new AccountRepository(new AccountDataAccess(sqliteConnectionCreator), notificationService);

            paymentManager = new PaymentManager(
                new PaymentRepository(new PaymentDataAccess(sqliteConnectionCreator),
                    new RecurringPaymentDataAccess(sqliteConnectionCreator),
                    accountRepository,
                    new CategoryRepository(new CategoryDataAccess(sqliteConnectionCreator), notificationService),
                    notificationService),
                accountRepository,
                null);
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                paymentManager.ClearPayments();
                Tile.UpdateMainTile();
            }
            catch (Exception ex)
            {
                Insights.Report(ex);
            }
        }
    }
}