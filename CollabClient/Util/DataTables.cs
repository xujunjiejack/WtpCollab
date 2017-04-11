using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CollabClient
{
    public class DataTables: ObservableCollection<Table>
    {

        private Table _selectedTable;

        public Table SelectedTable
        {
            get
            {
                return _selectedTable;
                
            }

            set
            {
                _selectedTable = value;

            }
        }

        public DataTables()
        {
        
        }

        public DataTables(String a)
        {
            Add(new Table("calc_4_hb_pb_f"));
            Add(new Table("calc_4_ei_t"));
            Add(new Table("calc_4_dt_f"));
            Add(new Table("calc_4_hs_t"));
            Add(new Table("a"));
            Add(new Table("b"));
            Add(new Table("c"));
            Add(new Table("d"));
            Add(new Table("e"));   
        }


        public String[] getAllTableNames(){
            if (this.Count == 0)
            {
                throw new ArgumentException("No element in the table lists. Please add table to your list");
            }

            List<String> tableNames = new List<String>();
            
            foreach (Table t in this)
            {
                tableNames.Add(t.TableName);
            }

            return tableNames.ToArray();
        }
    
    }

}
