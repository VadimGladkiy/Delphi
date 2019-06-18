using BaseFilterAssembly;
using Delphi.Util;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace Delphi
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region static_vars
        private static string yearAssemblyPath = "../../../YearFilterAssembly/bin/Debug/YearFilterAssembly.dll";
        private static string yearFilterClass = "YearFilterAssembly.YearFilter";

        private static string countryAssemblyPath = "../../../CountryFilterAssembly/bin/Debug/CountryFilterAssembly.dll";
        private static string countryFilterClass = "CountryFilterAssembly.CountryFilter";

        private static string genreAssemblyPath = "../../../GenreFilterAssembly/bin/Debug/GenreFilterAssembly.dll";
        private static string genreFilterClass = "GenreFilterAssembly.GenreFilter";

        private static string actorsAssemblyPath = "../../../ActorsFilterAssembly/bin/Debug/ActorsFilterAssembly.dll";
        private static string actorsFilterClass = "ActorsFilterAssembly.ActorsFilter";

        private static string directorAssemblyPath = "../../../DirectorFilterAssembly/bin/Debug/DirectorFilterAssembly.dll";
        private static string directorFilterClass = "DirectorFilterAssembly.DirectorFilter";
        #endregion


        // document reference
        private XElement xElement;

        // list of filtered movies in memory
        private List<Models.Movie> movies;

        /// <summary>
        /// assemblies in memory
        /// </summary>
        private Dictionary<string, Assembly> assemblies;

        private MethodInfo[] methods;

        public MainWindow()
        {
            InitializeComponent();

            // menu. no file to work with it. 
            fileParticular.Text = "file not chosen";

            assemblies = new Dictionary<string, Assembly>();

            methods = typeof(BaseFilter).GetMethods();

        }

        #region menu_common
        private void FileLoadMenu_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Xml files (*.xml)|*.xml";

            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // menu. the particular file source. 
                fileParticular.Text = dlg.FileName;

                xElement = XElement.Load(dlg.FileName);

                var serializer = new Serializer();
                movies = serializer.Deserialize<Models.Movies>(xElement.Elements().ToList())
                    .MoviesArray.ToList();

                PrepareRendering();
            }
        }


        private void FileUnLoadMenu_Click(object sender, RoutedEventArgs e)
        {

            // destroy file in memory
            xElement = null;

            // menu. no file to work with it. 
            fileParticular.Text = "file not chosen";

            // unload all assemblies, set dictionary to be empty.
            assemblies = new Dictionary<string, Assembly>();
        }


        private void InfoRead_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Some info about the program");
        }


        private void ProgramExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region menu_filters
        private void YearLoad_Click(object sender, RoutedEventArgs e)
        {
            MenuItemHandler(sender,
                yearAssemblyPath,
                yearFilterClass,
                FilterEnums.year);
        }


        private void CountryLoad_Click(object sender, RoutedEventArgs e)
        {
            MenuItemHandler(sender,
                countryAssemblyPath,
                countryFilterClass,
                FilterEnums.country);
        }


        private void GenreLoad_Click(object sender, RoutedEventArgs e)
        {
            MenuItemHandler(sender,
                genreAssemblyPath,
                genreFilterClass,
                FilterEnums.genre);
        }


        private void ActorsLoad_Click(object sender, RoutedEventArgs e)
        {
            MenuItemHandler(sender,
                actorsAssemblyPath,
                actorsFilterClass,
                FilterEnums.actor);
        }


        private void DirectorLoad_Click(object sender, RoutedEventArgs e)
        {
            MenuItemHandler(sender,
                directorAssemblyPath,
                directorFilterClass,
                FilterEnums.director);
        }


        private void MenuItemHandler(object sender, string assemblyPath, string className, FilterEnums filterBy)
        {
            var mi = (MenuItem)sender;
            if (xElement == null)
            {
                mi.IsChecked = false;
                MessageBox.Show("File not chosen");
            }
            else
            {
                if (mi.IsChecked == true)
                {
                    // load dll dynamically
                    var assembly = Assembly.LoadFrom(assemblyPath);

                    var type = assembly.GetType(className);
                    var instance = Activator.CreateInstance(type);

                    MethodInfo[] methods = type.GetMethods();

                    methods.Single(x => x.Name == "InjectElement").Invoke(instance, new object[] { xElement });
                    var list = (IEnumerable<string>)methods.Single(x => x.Name == "GetFilters").Invoke(instance, new object[] { filterBy.ToString() });

                    // save assembly reference
                    assemblies.Add(className, assembly);

                    // create a visual panel for the filter 
                    FillFilterColumn(list, filterBy, className.Split('.')[1]);
                }
                else
                {
                    // delete a visual panel for the filter
                    EraseFromFilterColumn(filterBy);

                    // remove assembly reference
                    assemblies.Remove(className);
                }
            }
        }


        private void FillFilterColumn(IEnumerable<string> list, FilterEnums filterBy, string classType)
        {
            WrapPanel wp = new WrapPanel();
            wp.Name = filterBy.ToString();
            foreach (var elem in list)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Name = filterBy.ToString();
                TextBlock tb = new TextBlock();
                tb.Text = elem;
                checkBox.Content = tb;
                checkBox.Click += DoFiltering_Click;

                wp.Children.Add(checkBox);
            }
            FiltersColumn.Children.Add(wp);
        }


        private void DoFiltering_Click(object sender, RoutedEventArgs e)
        {
            // get all checked
            List<CheckBox> checkBoxes = new List<CheckBox>();

            foreach (WrapPanel wp in FiltersColumn.Children)
            {
                foreach (CheckBox ch in wp.Children)
                {
                    if (ch.IsChecked.HasValue)
                    {
                        if (ch.IsChecked.Value)
                        {
                            checkBoxes.Add(ch);
                        }
                    }
                }
            }

            if (checkBoxes.Count > 0)
            {

                var boxes = new List<Models.Movie>();

                List<Func<Models.Movie, bool>> lFunc = new List<Func<Models.Movie,bool>>();


                List<string> yearWords = new List<string>();
                List<string> countryWords = new List<string>();
                List<string> genreWords = new List<string>();
                List<string> directorWords = new List<string>();
                List<string> actorWords = new List<string>();

                // fill the words lists
                foreach (CheckBox check in checkBoxes)
                {
                    var filterName = GetFilterEnumsItem(check.Name);
                    var textBlock = (TextBlock)check.Content;
                    var filterBy = textBlock.Text;

                    switch (filterName)
                    {
                        case FilterEnums.year:
                            yearWords.Add(filterBy);
                            break;
                        case FilterEnums.country:
                            countryWords.Add(filterBy);
                            break;
                        case FilterEnums.genre:
                            genreWords.Add(filterBy);
                            break;
                        case FilterEnums.director:
                            directorWords.Add(filterBy);
                            break;
                        case FilterEnums.actor:
                            actorWords.Add(filterBy);
                            break;
                    }
                }

                // form the queries
                if (yearWords.Count > 0)
                {
                    Func<Models.Movie, bool> yFunc = x => yearWords.Any(y => 
                        x.Year.ToString() == y);
                    lFunc.Add(yFunc);
                }
                if (countryWords.Count > 0)
                {
                    Func<Models.Movie, bool> cFunc = x => countryWords.Any(y => 
                        x.Country == y);
                    lFunc.Add(cFunc);
                }
                if (genreWords.Count > 0)
                {
                    Func<Models.Movie, bool> gFunc = x => genreWords.Any( y =>
                        x.Genre == y);
                    lFunc.Add(gFunc);
                }
                if (directorWords.Count > 0)
                {
                    List<Tuple<string, string>> lTuples = new List<Tuple<string, string>>();

                    foreach (var item in directorWords)
                    {
                        var name = item.Split(' '); 
                        Tuple<string, string> t = 
                            new Tuple<string, string>(name[0],name[1]);
                        lTuples.Add(t);
                    }

                    Func<Models.Movie, bool> dFunc = x => lTuples.Any(y =>
                       x.Director.first_name == y.Item1 &&
                       x.Director.last_name == y.Item2);
                    lFunc.Add(dFunc);
                }
                if (actorWords.Count > 0)
                {
                    List<Tuple<string, string>> lTuples = new List<Tuple<string, string>>();

                    foreach (var item in actorWords)
                    {
                        var name = item.Split(' ');

                        if (name.Count() > 2)
                        {
                            name[1] = name[1] + " " + name[2];
                        }

                        Tuple<string, string> t =
                            new Tuple<string, string>(name[0], name[1]);
                        lTuples.Add(t);
                    }

                    Func<Models.Movie, bool> aFunc = x => lTuples.Any(y =>
                       x.Actors.Any(z =>
                                   z.first_name == y.Item1 &&
                                   z.last_name == y.Item2));
                    lFunc.Add(aFunc);
                }

                Func<Models.Movie, bool> prev;
                Func<Models.Movie, bool> next;

                Func<Models.Movie, bool> resultQuery = x => lFunc[0](x);

                if (lFunc.Count > 1)
                {
                    if (lFunc.Count == 2)
                    {
                        resultQuery = x => lFunc[0](x) && lFunc[1](x);
                    }else
                    if (lFunc.Count == 3)
                    {
                        resultQuery = x => lFunc[0](x) && lFunc[1](x) && lFunc[2](x);
                    }else
                    if (lFunc.Count == 4)
                    {
                        resultQuery = x => lFunc[0](x) && lFunc[1](x) && lFunc[2](x) && lFunc[3](x);
                    }else
                    if (lFunc.Count == 5)
                    {
                        resultQuery = x => lFunc[0](x) && lFunc[1](x) && lFunc[2](x) && lFunc[3](x) && lFunc[4](x);
                    }
                }
                else
                {
                    resultQuery = lFunc[0];
                }
                
                boxes = movies.Where(resultQuery).ToList();

                PrepareRendering(boxes);
            }
            else
            {
                PrepareRendering();
            }
            // render the movies to UI
        }


        private void EraseFromFilterColumn(FilterEnums filterBy)
        {
            var length = FiltersColumn.Children.Count;

            for (int i = 0; i < length; i++)
            {
                var wp = FiltersColumn.Children[i] as WrapPanel;
                if (wp != null)
                {
                    if (wp.Name == filterBy.ToString())
                    {
                        FiltersColumn.Children.Remove(wp);
                        DoFiltering_Click(new { }, new RoutedEventArgs());
                        break;
                    }
                }
            }
        }


        private List<Models.Movie> FilterNodes(string className, string filterBy)
        {
            var moviesNodes = xElement.Elements("movie");

            var kpv = assemblies.Single(x => x.Key.Contains(className));

            var assembly = kpv.Value;

            var objectType = assembly.GetType(kpv.Key);

            var filterInstance = Activator.CreateInstance(objectType);

            moviesNodes = (IEnumerable<XElement>)methods.Single(x => x.Name == "GetFiltered")
                    .Invoke(filterInstance, new object[] { moviesNodes, filterBy });

            var serializer = new Serializer();
            return serializer.Deserialize<Models.Movies>(moviesNodes.ToList()).MoviesArray.ToList();

        }


        private FilterEnums GetFilterEnumsItem(string input)
        {
            switch (input)
            {
                case "year":
                    return FilterEnums.year;
                case "country":
                    return FilterEnums.country;
                case "genre":
                    return FilterEnums.genre;
                case "director":
                    return FilterEnums.director;
                case "actor":
                    return FilterEnums.actor;
                default: throw new NotImplementedException();
            }
        }
        #endregion

        #region ui_output

        public void PrepareRendering(List<Models.Movie> collection)
        {
            render(collection);
        }

        public void PrepareRendering()
        {
            render(movies);
        }

        private void render(List<Models.Movie> collection)
        {
            // remove before write new
            DataColumn.Children.RemoveRange(0, DataColumn.Children.Count);

            foreach (var movie in collection)
            {
                // define & set title
                var movieTitle = new TextBlock();
                movieTitle.Text = movie.Title;

                var sp = new StackPanel();
                sp.HorizontalAlignment = HorizontalAlignment.Center;

                sp.Children.Add(movieTitle);

                // define & set info
                var gridInfo = CreateDynamicWPFGrid(500, 5, 2, true, 3);

                // add year 

                var year = new TextBlock();
                year.Text = "Year";

                var movieYear = new TextBlock();
                movieYear.Text = movie.Year.ToString();

                Grid.SetRow(year, 0);
                Grid.SetColumn(year, 0);
                gridInfo.Children.Add(year);

                Grid.SetRow(movieYear, 0);
                Grid.SetColumn(movieYear, 1);
                gridInfo.Children.Add(movieYear);

                // add country
                var country = new TextBlock();
                country.Text = "country";

                var movieCountry = new TextBlock();
                movieCountry.Text = movie.Country;

                Grid.SetRow(country, 1);
                Grid.SetColumn(country, 0);
                gridInfo.Children.Add(country);

                Grid.SetRow(movieCountry, 1);
                Grid.SetColumn(movieCountry, 1);
                gridInfo.Children.Add(movieCountry);

                // add genre
                var genre = new TextBlock();
                genre.Text = "genre";

                var movieGenre = new TextBlock();
                movieGenre.Text = movie.Genre;

                Grid.SetRow(genre, 2);
                Grid.SetColumn(genre, 0);
                gridInfo.Children.Add(genre);

                Grid.SetRow(movieGenre, 2);
                Grid.SetColumn(movieGenre, 1);
                gridInfo.Children.Add(movieGenre);

                // add summary
                var summary = new TextBlock();
                summary.Text = "summary";

                var movieSummary = new TextBlock();
                movieSummary.Text = movie.Summary;
                movieSummary.TextWrapping = TextWrapping.Wrap;

                Grid.SetRow(summary, 3);
                Grid.SetColumn(summary, 0);
                gridInfo.Children.Add(summary);

                Grid.SetRow(movieSummary, 3);
                Grid.SetColumn(movieSummary, 1);
                gridInfo.Children.Add(movieSummary);

                // add director
                var director = new TextBlock();
                director.Text = "director";

                var movieDirector = new TextBlock();
                movieDirector.Text = movie.Director.first_name + " " +
                                  movie.Director.last_name + ", " +
                                  movie.Director.birth_date;

                Grid.SetRow(director, 4);
                Grid.SetColumn(director, 0);
                gridInfo.Children.Add(director);

                Grid.SetRow(movieDirector, 4);
                Grid.SetColumn(movieDirector, 1);
                gridInfo.Children.Add(movieDirector);

                // compose
                DataColumn.Children.Add(sp);

                foreach (var item in gridInfo.Children)
                {
                    var temp = (TextBlock)item;
                    temp.HorizontalAlignment = HorizontalAlignment.Center;
                }

                DataColumn.Children.Add(gridInfo);

                var countOfActors = movie.Actors.Length;
                var gridInfoActors = CreateDynamicWPFGrid(500, countOfActors * 2, 2, false, 0);

                var row = 0;
                foreach (var item in movie.Actors)
                {
                    // add actor
                    var actor = new TextBlock();
                    actor.Text = "actor";

                    Grid.SetRow(actor, row);
                    Grid.SetColumn(actor, 0);
                    gridInfoActors.Children.Add(actor);

                    var actorName = new TextBlock();
                    actorName.Text = item.first_name + " " +
                                   item.last_name + ", " +
                                   item.birth_date;

                    Grid.SetRow(actorName, row);
                    Grid.SetColumn(actorName, 1);
                    gridInfoActors.Children.Add(actorName);

                    //add role
                    var role = new TextBlock();
                    role.Text = "role";

                    Grid.SetRow(role, row + 1);
                    Grid.SetColumn(role, 0);
                    gridInfoActors.Children.Add(role);

                    var actorRole = new TextBlock();
                    actorRole.Text = item.role;

                    Grid.SetRow(actorRole, row + 1);
                    Grid.SetColumn(actorRole, 1);
                    gridInfoActors.Children.Add(actorRole);

                    row += 2;
                }

                foreach (var item in gridInfoActors.Children)
                {
                    var temp = (TextBlock)item;
                    temp.HorizontalAlignment = HorizontalAlignment.Center;
                }

                DataColumn.Children.Add(gridInfoActors);
            }
        }

        private Grid CreateDynamicWPFGrid(int width, int rows, int columns, bool highRow, int highRowNumber)
        {
            // Create the Grid

            Grid DynamicGrid = new Grid();

            DynamicGrid.Width = width;

            DynamicGrid.HorizontalAlignment = HorizontalAlignment.Left;

            DynamicGrid.VerticalAlignment = VerticalAlignment.Top;

            DynamicGrid.ShowGridLines = true;

            DynamicGrid.Background = new SolidColorBrush(Colors.LightSteelBlue);

            // Create Columns
            for (int i = 0; i < columns; i++)
            {
                ColumnDefinition gridCol = new ColumnDefinition();
                if (i == 0)
                {
                    gridCol.Width = new GridLength(100);
                }
                else
                {
                    ;
                }
                DynamicGrid.ColumnDefinitions.Add(gridCol);
            }

            // Create Rows

            for (int i = 0; i < rows; i++)
            {
                RowDefinition gridRow = new RowDefinition();
                if (highRow)
                {
                    if (i == highRowNumber)
                    {
                        gridRow.Height = new GridLength(220);
                    }
                }
                else
                {
                    gridRow.Height = new GridLength(20);
                }
                DynamicGrid.RowDefinitions.Add(gridRow);
            }

            return DynamicGrid;
        }
        #endregion
    }
}
