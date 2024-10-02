using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz.Model
{
    public class Quiz
    {
        public string Name { get; set; }
        public ObservableCollection<Question> Questions { get; set; } = new ObservableCollection<Question>();
    }
}
