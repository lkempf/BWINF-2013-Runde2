using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;

namespace PLG
{
    public partial class MainWindow : Window
    {
        ObservableCollection<Person> persons;
        Dictionary<int, CheckBox> personCheckBoxes;
        BackgroundWorker backgroundWorker = new BackgroundWorker();

        int lastID = 0;
        bool ignoreEvent = false;
        bool hasChanged;

        public MainWindow()
        {
            InitializeComponent();
            persons = new ObservableCollection<Person>();
            personCheckBoxes = new Dictionary<int, CheckBox>();

            PersonsListBox.ItemsSource = persons;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
        }

        private void newPersonButton_Click(object sender, RoutedEventArgs e)
        {
            Person newPerson = new Person(lastID++);
            newPerson.PropertyChanged += newPerson_PropertyChanged;
            persons.Add(newPerson);

            CheckBox personCheckBox = new CheckBox();
            personCheckBox.Content = newPerson.Name;
            personCheckBoxes.Add(lastID - 1, personCheckBox);
            relationsListBox.Items.Add(personCheckBox);

            personCheckBox.Checked += personCheckBox_Checked;
            personCheckBox.Unchecked += personCheckBox_Unchecked;

            PersonsListBox.SelectedItem = newPerson;

            hasChanged = true;
        }

        void personCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!ignoreEvent)
            {
                CheckBox currentCheckBox = sender as CheckBox;
                Person currentPerson = PersonsListBox.SelectedItem as Person;

                currentPerson.Neighbors.Remove(persons.Where(person => person.ID == personCheckBoxes.FindKeyByValue(currentCheckBox)).First());
                persons.Where(person => person.ID == personCheckBoxes.FindKeyByValue(currentCheckBox)).First().Neighbors.Remove(currentPerson);
            }

            hasChanged = true;
        }

        void personCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!ignoreEvent)
            {
                CheckBox currentCheckBox = sender as CheckBox;
                Person currentPerson = PersonsListBox.SelectedItem as Person;

                if (!currentPerson.Neighbors.Contains(persons.Where(person => person.ID == personCheckBoxes.FindKeyByValue(currentCheckBox)).First()))
                {
                    currentPerson.Neighbors.Add(persons.Where(person => person.ID == personCheckBoxes.FindKeyByValue(currentCheckBox)).First());
                    persons.Where(person => person.ID == personCheckBoxes.FindKeyByValue(currentCheckBox)).First().Neighbors.Add(currentPerson);
                }
            }

            hasChanged = true;
        }

        void newPerson_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
                personCheckBoxes[(sender as Person).ID].Content = (sender as Person).Name;
        }

        private void PersonsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ignoreEvent = true;

            if (PersonsListBox.SelectedItem != null)
            {
                Person current = (Person)PersonsListBox.SelectedItem;
                detailsBox.DataContext = current;

                foreach (CheckBox checkBox in personCheckBoxes.Values)
                {
                    checkBox.IsEnabled = true;
                    checkBox.IsChecked = false;
                }

                personCheckBoxes[current.ID].IsEnabled = false;

                foreach (Person person in current.Neighbors)
                {
                    personCheckBoxes[person.ID].IsChecked = true;
                } 
            }

            ignoreEvent = false;
        }

        private void removePersonButton_Click(object sender, RoutedEventArgs e)
        {
            if (PersonsListBox.SelectedIndex != -1)
            {
                Person selectedPerson = (Person)PersonsListBox.SelectedItem;

                foreach (Person neighbor in selectedPerson.Neighbors)
                    neighbor.Neighbors.Remove(selectedPerson);

                relationsListBox.Items.Remove(personCheckBoxes[(selectedPerson).ID]);
                personCheckBoxes.Remove((selectedPerson).ID);
                persons.Remove(selectedPerson);
                hasChanged = true;
            }
        }

        private void openFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            persons.Clear();

            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.ShowDialog();

            try
            {
                using (StreamReader reader = new StreamReader(fileDialog.OpenFile()))
                {
                    while (reader.ReadLine() != "#nodes section") ;

                    int nodesCount = int.Parse(reader.ReadLine());
                    persons = new ObservableCollection<Person>();

                    for (int i = 0; i < nodesCount; i++)
                    {
                        string cur = reader.ReadLine();
                        string[] splited = cur.Split(new char[2] { '{', '}' });
                        Person p = new Person(i) { Name = splited[1] };
                        persons.Add(p);
                    }

                    reader.ReadLine();
                    reader.ReadLine();

                    int edgesCount = int.Parse(reader.ReadLine());

                    for (int i = 0; i < edgesCount; i++)
                    {
                        string cur = reader.ReadLine();
                        string[] splited = cur.Split(new char[1] { ' ' });

                        int x = int.Parse(splited[0]) - 1, y = int.Parse(splited[1]) - 1;

                        persons[x].Neighbors.Add(persons[y]);
                        persons[y].Neighbors.Add(persons[x]);
                    }
                }
            }

            //File dialog closed without selecting anything
            catch (InvalidOperationException)
            { }

            lastID = persons.Count;

            hasChanged = true;

            BuildGUI();
        }

        private void BuildGUI()
        {
            personCheckBoxes.Clear();
            relationsListBox.Items.Clear();

            ignoreEvent = true;

            foreach (Person p in persons)
            {
                p.PropertyChanged += newPerson_PropertyChanged;

                CheckBox personCheckBox = new CheckBox();
                personCheckBox.Content = p.Name;
                personCheckBoxes.Add(p.ID, personCheckBox);
                relationsListBox.Items.Add(personCheckBox);

                personCheckBox.Checked += personCheckBox_Checked;
                personCheckBox.Unchecked += personCheckBox_Unchecked;
            }

            ignoreEvent = false;

            PersonsListBox.ItemsSource = null;
            PersonsListBox.ItemsSource = persons;
            PersonsListBox.SelectedIndex = 0;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            relationsListBox.MaxHeight = this.ActualHeight - 117;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            persons.Clear();
            int cnt = 1000;

            for (int i = 0; i < cnt; i++)
            {
                persons.Add(new Person(i));
            }

            for (int i = 0; i < cnt; i++)
            {
                for (int a = cnt - 1; a > persons[a].Neighbors.Count - 1; a--)
                {
                    if (i != a)
                    {
                        persons[i].Neighbors.Add(persons[a]);
                        persons[a].Neighbors.Add(persons[i]);
                    }
                }
            }

            BuildGUI();
        }

        private void saveFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog fileDialog = new Microsoft.Win32.SaveFileDialog();
            fileDialog.OverwritePrompt = true;
            fileDialog.ShowDialog();

            try
            {
                using (StreamWriter writer = new StreamWriter(File.Create(fileDialog.FileName)))
                {
                    writer.WriteLine("#nodes section");
                    writer.WriteLine(persons.Count);

                    foreach (Person person in persons)
                        writer.WriteLine("{" + person.Name + "}");

                    writer.WriteLine();
                    writer.WriteLine("#edges section");

                    int relationsCnt = 0;
                    foreach (Person person in persons)
	                {
		                relationsCnt += person.Neighbors.Count;
                        person.Flagged = false;
	                }
                    relationsCnt = relationsCnt / 2;

                    writer.WriteLine(relationsCnt);

                    Stack<Person> stack = new Stack<Person>();
                    foreach (List<Person> component in Plg.findConnectedComponents(persons))
                    {
                        stack.Push(component[0]);

                        while (stack.Count > 0)
                        {
                            Person currentPerson = stack.Pop();
                            currentPerson.Flagged = true;

                            foreach (Person neighbor in currentPerson.Neighbors)
                            {
                                if (!neighbor.Flagged)
                                {
                                    writer.WriteLine(String.Format("{0} {1}", currentPerson.ID + 1, neighbor.ID + 1));
                                    stack.Push(neighbor);
                                }
                            }
                        }
                    }
                }
            }

            //File dialog closed without selecting anything
            catch (ArgumentException)
            { }
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Plg plg;
            lock (persons)
            {
                plg = new Plg(persons);
            }

            Person[] output;
            string outString;

            if (!plg.TestIntervalGraph(out output))
            {
                if (output.Length == 3)
                    outString = "AT gefunden: \r\n";
                else
                    outString = "Sehnenloser Kreis gefunden:\r\n";

                foreach (Person p in output)
                {
                    outString += p.Name;
                    outString += "\r\n";
                }

                if (output.Length == 3)
                {
                    var result = plg.FindATPaths(output);
                    foreach (var list in result)
                    {
                        outString += "Pfad:\r\nStart:\r\n";

                        foreach (Person p in list)
                            outString += p.Name + "\r\n";

                        outString += ":Ende\r\n";
                    }
                }
            }

            else
            {
                outString = "Intervalldarstellung erzeugt:\r\n";
                foreach (Person p in persons)
                    outString += String.Format("{0}: Von {1} bis {2}\r\n", p.Name, p.Birth, p.Death);
            }

            e.Result = outString;
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            outputTextBox.Text = (string)e.Result;
        }

        private void outputTab_GotFocus(object sender, RoutedEventArgs e)
        {
            if (hasChanged)
            {
                outputTextBox.Text = "Bitte Warten...";
                backgroundWorker.RunWorkerAsync();
                hasChanged = false;
            }
        }
    }
}
