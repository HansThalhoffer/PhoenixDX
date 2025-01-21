using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PhoenixModel.Database.DatabaseQueue;

namespace PhoenixModel.Database {


    public class DatabaseQueue : ConcurrentQueue<DatabaseQueueItem> {

        public enum DatabaseQueueCommand {
            Update, Insert, Delete, Save = Update
        }

        public class DatabaseQueueItem {
            public DatabaseQueueCommand Command;
            public IDatabaseTable Table;
            public DatabaseQueueItem(IDatabaseTable table, DatabaseQueueCommand command) {
                Command = command;
                Table = table;
            }
        }

        public void Enqueue(IDatabaseTable table) {
            base.Enqueue(new DatabaseQueueItem(table, DatabaseQueueCommand.Update));
        }

        public void Save(IDatabaseTable table) {
            base.Enqueue(new DatabaseQueueItem(table, DatabaseQueueCommand.Update));
        }

        public void Insert(IDatabaseTable table) {
            base.Enqueue(new DatabaseQueueItem(table, DatabaseQueueCommand.Insert));
        }

        public void Delete(IDatabaseTable table) {
            base.Enqueue(new DatabaseQueueItem(table, DatabaseQueueCommand.Delete));
        }
    }
}
