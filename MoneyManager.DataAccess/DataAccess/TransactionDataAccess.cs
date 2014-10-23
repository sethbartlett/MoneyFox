﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.Practices.ServiceLocation;
using MoneyManager.DataAccess.DataAccess;
using MoneyManager.DataAccess.Model;
using PropertyChanged;

namespace MoneyManager.DataAccess
{
    [ImplementPropertyChanged]
    internal class TransactionDataAccess : AbstractDataAccess<FinancialTransaction>
    {
        public TransactionDataAccess()
        {
            LoadList();
        }

        public ObservableCollection<FinancialTransaction> AllTransactions { get; set; }

        public FinancialTransaction SelectedTransaction { get; set; }

        private AccountDataAccess AccountDataAccess
        {
            get { return ServiceLocator.Current.GetInstance<AccountDataAccess>(); }
        }

        private RecurringTransactionDataAccess RecurringTransactionData
        {
            get { return ServiceLocator.Current.GetInstance<RecurringTransactionDataAccess>(); }
        }

        private static TransactionListUserControlViewModel TransactionListUserControlView
        {
            get { return ServiceLocator.Current.GetInstance<TransactionListUserControlViewModel>(); }
        }

        protected override void SaveToDb(FinancialTransaction transaction)
        {
            SaveToDb(transaction, false);
        }

        public void SaveToDb(FinancialTransaction transaction, bool skipRecurring)
        {
            using (var dbConn = ConnectionFactory.GetDbConnection())
            {
                if (AllTransactions == null)
                {
                    AllTransactions = new ObservableCollection<FinancialTransaction>();
                }

                AccountDataAccess.AddTransactionAmount(transaction);
                if (!skipRecurring && transaction.IsRecurring)
                {
                    RecurringTransactionData.Save(transaction);
                }

                AllTransactions.Add(transaction);
                AllTransactions = new ObservableCollection<FinancialTransaction>(AllTransactions.OrderBy(x => x.Date));

                RefreshRelatedTransactions(transaction);

                dbConn.Insert(transaction, typeof (FinancialTransaction));
            }
        }

        private void RefreshRelatedTransactions(FinancialTransaction transaction)
        {
            if (AccountDataAccess.SelectedAccount == transaction.ChargedAccount)
            {
                TransactionListUserControlView.SetRelatedTransactions(transaction.ChargedAccountId);
            }
        }

        protected override void DeleteFromDatabase(FinancialTransaction transaction)
        {
            using (var dbConn = ConnectionFactory.GetDbConnection())
            {
                AccountDataAccess.RemoveTransactionAmount(transaction);

                AllTransactions.Remove(transaction);
                RefreshRelatedTransactions(transaction);
                dbConn.Delete(transaction);

                CheckForRecurringTransaction(transaction,
                    () => RecurringTransactionData.Delete(transaction.ReccuringTransactionId.Value));
            }
        }

        public void DeleteAssociatedTransactionsFromDatabase(int accountId)
        {
            using (var dbConn = ConnectionFactory.GetDbConnection())
            {
                if (AllTransactions == null)
                {
                    AllTransactions = new ObservableCollection<FinancialTransaction>();
                }

                var transactions = dbConn.Table<FinancialTransaction>()
                    .Where(x => x.ChargedAccountId == accountId)
                    .ToList();

                foreach (var transaction in transactions)
                {
                    AllTransactions.Remove(transaction);
                    dbConn.Delete(transaction);
                }
            }
        }

        protected override void GetListFromDb()
        {
            using (var dbConn = ConnectionFactory.GetDbConnection())
            {
                AllTransactions = new ObservableCollection<FinancialTransaction>
                    (dbConn.Table<FinancialTransaction>().ToList());
            }
        }

        private void CheckIfRecurringWasRemoved(FinancialTransaction transaction)
        {
            if (!transaction.IsRecurring && transaction.ReccuringTransactionId != null)
            {
                RecurringTransactionData.Delete(transaction.ReccuringTransactionId.Value);
            }
        }

        public IEnumerable<FinancialTransaction> GetRelatedTransactions(int accountId)
        {
            return AllTransactions
                .Where(x => x.ChargedAccountId == accountId || x.TargetAccountId == accountId)
                .ToList();
        }

        protected override async void UpdateItem(FinancialTransaction transaction)
        {
            using (var dbConn = ConnectionFactory.GetDbConnection())
            {
                CheckIfRecurringWasRemoved(transaction);

                AccountDataAccess.AddTransactionAmount(transaction);
                dbConn.Update(transaction);

                await CheckForRecurringTransaction(transaction, () => RecurringTransactionData.Update(transaction));
            }
        }

        private async Task CheckForRecurringTransaction(FinancialTransaction transaction,
            Action recurringTransactionAction)
        {
            if (!transaction.IsRecurring) return;

            var dialog =
                new MessageDialog(Utilities.GetTranslation("ChangeSubsequentTransactionsMessage"),
                    Utilities.GetTranslation("ChangeSubsequentTransactionsTitle"));

            dialog.Commands.Add(new UICommand(Utilities.GetTranslation("RecurringLabel")));
            dialog.Commands.Add(new UICommand(Utilities.GetTranslation("JustThisLabel")));

            dialog.DefaultCommandIndex = 1;

            IUICommand result = await dialog.ShowAsync();

            if (result.Label == Utilities.GetTranslation("RecurringLabel"))
            {
                recurringTransactionAction();
            }
        }

        public void ClearTransaction()
        {
            IEnumerable<FinancialTransaction> transactions = GetUnclearedTransactions();
            foreach (FinancialTransaction transaction in transactions)
            {
                AccountDataAccess.AddTransactionAmount(transaction);
            }
        }

        public IEnumerable<FinancialTransaction> GetUnclearedTransactions()
        {
            using (var dbConn = ConnectionFactory.GetDbConnection())
            {
                return dbConn.Table<FinancialTransaction>().Where(x => x.Cleared == false
                                                                       && x.Date <= DateTime.Now).ToList();
            }
        }

        public List<FinancialTransaction> LoadRecurringList()
        {
            using (var db = ConnectionFactory.GetDbConnection())
            {
                //Have to make a list before apply the where statements
                return db.Table<FinancialTransaction>()
                    .ToList()
                    .Where(x => x.IsRecurring)
                    .Where(x => x.RecurringTransaction.IsEndless || x.RecurringTransaction.EndDate >= DateTime.Now.Date)
                    .ToList();
            }
        }
    }
}