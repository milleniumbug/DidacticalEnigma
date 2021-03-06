﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DidacticalEnigma.Core.Models.LanguageService;
using Utility.Utils;

#if AVALONIA
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
#else
using System.Windows.Controls;
using System.Windows.Threading;
#endif

namespace DidacticalEnigma.ViewModels
{
    public class KanjiRadicalLookupControlVM : INotifyPropertyChanged, IDisposable
    {
        public class RadicalVM : INotifyPropertyChanged
        {
            public CodePoint CodePoint { get; }

            public int StrokeCount { get; }

            public string Name { get; }

            public Visibility Visible
            {
                get
                {
                    if (enabled)
                        return Visibility.Visible;
                    if (lookupVm.HideNonMatchingRadicals)
                        return Visibility.Collapsed;
                    return Visibility.Visible;
                }
            }

            private bool selected;
            public bool Selected
            {
                get => selected;
                set
                {
                    if (selected == value)
                        return;

                    selected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Visible));
                }
            }

            private bool enabled;
            public bool Enabled
            {
                get => enabled;
                set
                {
                    if (enabled == value)
                        return;

                    enabled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Visible));
                }
            }

            public bool Highlighted => false;

            private readonly KanjiRadicalLookupControlVM lookupVm;

            public RadicalVM(JDict.Radical radical, bool enabled, KanjiRadicalLookupControlVM lookupVm, string name)
            {
                CodePoint = CodePoint.FromInt(radical.CodePoint);
                StrokeCount = radical.StrokeCount;
                this.enabled = enabled;
                this.lookupVm = lookupVm;
                Name = name;
                lookupVm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(HideNonMatchingRadicals))
                    {
                        OnPropertyChanged(nameof(Visible));
                    }
                };
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Task task = Task.CompletedTask;
        private CancellationTokenSource addingTaskCancellationToken = null;

        public ICommand Reset { get; }

        public async void SetElements(IReadOnlyCollection<CodePoint> elements, Dispatcher dispatcher)
        {
            addingTaskCancellationToken?.Cancel();
            await task;

            addingTaskCancellationToken = new CancellationTokenSource();
            var token = addingTaskCancellationToken.Token;
            sortedKanji.Clear();
            const int n = 20;
            sortedKanji.AddRange(elements.Take(n));
            if (elements.Count <= n)
            {
                return;
            }

            var tcs = new TaskCompletionSource<bool>();
            task = tcs.Task;
            if (dispatcher != null)
            {
#pragma warning disable 4014
                Task.Run(() =>
#pragma warning restore 4014
                {
                    try
                    {
                        foreach (var chunk in elements.Skip(n).ChunkBy(400))
                        {
                            if (token.IsCancellationRequested)
                                break;
                            dispatcher.Invoke(() => { sortedKanji.AddRange(chunk); },
                                DispatcherPriority.ApplicationIdle,
                                token);
                        }
                    }
                    finally
                    {
                        tcs.SetResult(true);
                    }
                });
            }
            else
            {
                sortedKanji.AddRange(elements.Skip(n));
            }
        }

        public void SelectRadicals(IEnumerable<CodePoint> codePoints, Dispatcher dispatcher)
        {
            codePoints = codePoints.Materialize();
            if (!codePoints.Any())
            {
                foreach (var radical in Radicals)
                {
                    radical.Enabled = true;
                }

                SetElements(Array.Empty<CodePoint>(), dispatcher);
                return;
            }

            var lookup = this.lookup.SelectRadical(codePoints);
            SetElements(lookup.Kanji, dispatcher);
            for (var i = 0; i < radicals.Count; i++)
            {
                if(codePoints.Contains(radicals[i].CodePoint))
                    continue;

                radicals[i].Enabled = lookup.PossibleRadicals[i].Value;
            }
        }

        private bool hideNonMatchingRadicals = false;

        public bool HideNonMatchingRadicals
        {
            get => hideNonMatchingRadicals;
            set
            {
                if (hideNonMatchingRadicals == value)
                    return;
                hideNonMatchingRadicals = value;
                OnPropertyChanged();
            }
        }

        private string searchQueryText;

        public string SearchQueryText
        {
            get => searchQueryText;
            set
            {
                if (searchQueryText == value)
                    return;

                searchQueryText = value;
                OnPropertyChanged();
                QueryTextToSelection(value);
            }
        }

        private void QueryTextToSelection(string text)
        {
            results.Clear();
            results.AddRange(searcher.Search(SearchQueryText));
            
            foreach (var radicalVm in radicals)
            {
                radicalVm.Selected = results.Any(r => r.Radical == radicalVm.CodePoint);
            }
        }

        private void RadicalOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var radical = (RadicalVM) sender;
            if (e.PropertyName == nameof(RadicalVM.Selected))
            {
                if (radical.Selected)
                {
                    if (results.All(r => r.Radical != radical.CodePoint))
                    {
                        searchQueryText = searchQueryText + " " + radical.CodePoint;
                    }
                }
                else
                {
                    for (int i = results.Count - 1; i >= 0; i--)
                    {
                        if (results[i].Radical == radical.CodePoint)
                        {
                            searchQueryText = searchQueryText.Remove(results[i].Start, results[i].Length);
                        }
                    }
                }

                searchQueryText = searchQueryText.Trim();
                OnPropertyChanged(nameof(SearchQueryText));
                results.Clear();
                results.AddRange(searcher.Search(SearchQueryText));
            }
        }

        private List<RadicalSearcherResult> results = new List<RadicalSearcherResult>();

        public double Width { get; }

        public double Height { get; }

        private readonly ObservableBatchCollection<CodePoint> sortedKanji = new ObservableBatchCollection<CodePoint>();
        public IEnumerable<CodePoint> SortedKanji => sortedKanji;

        private readonly ObservableBatchCollection<RadicalVM> radicals = new ObservableBatchCollection<RadicalVM>();
        public IEnumerable<RadicalVM> Radicals => radicals;

        public KanjiRadicalLookupControlVM(
            KanjiRadicalLookup lookup,
            IKanjiProperties kanjiProperties,
            IRadicalSearcher searcher,
            IReadOnlyDictionary<CodePoint, string> textForms)
        {
            this.lookup = lookup;
            this.searcher = searcher;
            radicals.AddRange(lookup.AllRadicals.Join(
                kanjiProperties.Radicals,
                c => c.Utf32,
                r => r.CodePoint,
                (c, r) => new RadicalVM(r, enabled: true, this, textForms[c])));
            this.radicalCodePointLookup = radicals.ToDictionary(r => r.CodePoint, r => r);

            var tb = new TextBlock
            {
                FontSize = 24
            };
            foreach (var k in Radicals)
            {
                tb.Measure(new Size(100, 100));
                tb.Text = k.CodePoint.ToString();
                var size = tb.DesiredSize;
                Height = Math.Max(Height, size.Height);
                Width = Math.Max(Width, size.Width);
            }

            Height += 25;
            Width += 25;
            Height = Math.Max(Width, Height);
            Width = Math.Max(Width, Height);
            Reset = new RelayCommand(() => { SearchQueryText = ""; });

            foreach (var k in Radicals)
            {
                k.PropertyChanged += RadicalOnPropertyChanged;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public IEnumerable<IKanjiOrdering> SortingCriteria => lookup.SortingCriteria;

        private readonly KanjiRadicalLookup lookup;
        private readonly IRadicalSearcher searcher;
        private IReadOnlyDictionary<CodePoint, RadicalVM> radicalCodePointLookup;

        public int CurrentKanjiOrderingIndex
        {
            get => lookup.SortingCriteria.SelectedIndex;
            set
            {
                if (lookup.SortingCriteria.SelectedIndex == value)
                    return;

                lookup.SortingCriteria.SelectedIndex = value;
                OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            addingTaskCancellationToken?.Cancel();
        }
    }
}
