using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Globalization;

namespace DarkStyle
{
    public class HyperlinkLabel : StackPanel
    {

        static readonly Regex UrlRegex = new Regex(
            "(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]"
            , RegexOptions.Compiled);
        static readonly MouseEventArgs _MouseEventArgs = new MouseEventArgs(Mouse.PrimaryDevice, 0);

        readonly List<TextBlock> _TextBlocks = new List<TextBlock>();
        readonly DoubleAnimation _ToolTipOpenAnim = new DoubleAnimation(0, 1, new Duration(TimeSpan.Zero));

        private string _HyperlinkText = string.Empty;
        public string HyperlinkText
        {
            get => _HyperlinkText;
            set
            {
                if (_HyperlinkText != value)
                {
                    _HyperlinkText = value;
                    Children.Clear();
                    MatchCollection matchs = UrlRegex.Matches(_HyperlinkText);
                    if (matchs.Count == 0)
                        AddLabel(_HyperlinkText);
                    else
                    {
                        int index = 0;
                        foreach (Match match in matchs)
                        {
                            if (index < match.Index)
                            {
                                string label = _HyperlinkText.Substring(index, match.Index - index);
                                if (label[label.Length - 1] == ']')
                                {
                                    int si = label.LastIndexOf('[');
                                    if (si >= 0)
                                    {
                                        if (si > 0)
                                            AddLabel(label.Substring(0, si));
                                        AddTextBlock(label.Substring(si).Trim('[', ']'), match.Value);
                                        index = match.Index + match.Length;
                                        continue;
                                    }
                                }
                                AddLabel(label);
                                index = match.Index + match.Length;
                            }
                            else if (index == 0 && match.Index == 0)
                                index = match.Index + match.Length;
                            AddTextBlock(match.Value);
                        }
                        if (index < _HyperlinkText.Length - 1)
                            AddLabel(_HyperlinkText.Substring(index));

                    }
                }

            }
        }

        private Brush _HyperlinkBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0, 0x97, 0xFB));

        public Brush HyperlinkBrush
        {
            get => _HyperlinkBrush;
            set
            {
                if (_HyperlinkBrush != value)
                    foreach (var textblock in _TextBlocks)
                        textblock.Foreground = value;
                _HyperlinkBrush = value;
            }
        }

        public double OpiningSpeed
        {
            get => _ToolTipOpenAnim.Duration.TimeSpan.TotalMilliseconds;
            set => _ToolTipOpenAnim.Duration = new Duration(TimeSpan.FromMilliseconds(value));
        }

        void AddLabel(string label)
        {
            Children.Add(new TextBlock()
            {
                Foreground = Brushes.White,
                Text = label,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        void AddTextBlock(string text, string contenturl = null)
        {
            TextBlock textBlock = new TextBlock()
            {
                Foreground = HyperlinkBrush,
                Text = text,
                Tag = text,
                VerticalAlignment = VerticalAlignment.Center
            };

            ToolTip toolTip = new ToolTip()
            {
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x25, 0x25, 0x26)),
                Foreground = Brushes.White,
                Content = "按Ctrl键并单击可转到链接"
            };

            textBlock.ToolTip = toolTip;

            if (contenturl != null)
            {
                toolTip.Content += contenturl;
                textBlock.Tag = contenturl;
            }

            Typeface typeface = new Typeface(toolTip.FontFamily, toolTip.FontStyle, toolTip.FontWeight, toolTip.FontStretch);
            FormattedText formattedText = new FormattedText(toolTip.Content.ToString(), 
                CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, toolTip.FontSize, toolTip.Foreground, 1);
            toolTip.Tag = formattedText.Width;

            textBlock.MouseMove += TextBlock_MouseMove;
            textBlock.MouseLeave += TextBlock_MouseLeave;
            textBlock.MouseLeftButtonDown += TextBlock_MouseLeftButtonDown;
            textBlock.ToolTipOpening += TextBlock_ToolTipOpening;
            _TextBlocks.Add(textBlock);
            Children.Add(textBlock);
        }

        void TextBlock_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
            ToolTip toolTip = (sender as TextBlock).ToolTip as ToolTip;
            toolTip.RenderTransform = new ScaleTransform(0, 0, (double)toolTip.Tag / 2, 0);
            toolTip.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, _ToolTipOpenAnim);
            toolTip.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, _ToolTipOpenAnim);
        }

        void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                Process.Start((sender as TextBlock).Tag.ToString());
        }

        void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            (sender as TextBlock).TextDecorations.Clear();
        }

        void TextBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                Cursor = Cursors.Hand;
                (sender as TextBlock).TextDecorations.Add(TextDecorations.Underline);
            }
            else
            {
                Cursor = Cursors.Arrow;
                (sender as TextBlock).TextDecorations.Clear();
            }
        }

        void HyperlinkLabel_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                foreach (var textblock in _TextBlocks)
                {
                    if (textblock.IsMouseOver)
                    {
                        _MouseEventArgs.RoutedEvent = MouseLeaveEvent;
                        textblock.RaiseEvent(_MouseEventArgs);
                        break;
                    }
                }
            }
        }

        void HyperlinkLabel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                foreach (var textblock in _TextBlocks)
                {
                    if (textblock.IsMouseOver)
                    {
                        _MouseEventArgs.RoutedEvent = MouseMoveEvent;
                        textblock.RaiseEvent(_MouseEventArgs);
                        break;
                    }
                }
            }
        }

        void HyperlinkLabel_Loaded(object sender, RoutedEventArgs e)
        {
            Window root = Window.GetWindow(this);
            root.KeyDown += HyperlinkLabel_KeyDown;
            root.KeyUp += HyperlinkLabel_KeyUp;
        }

        public HyperlinkLabel()
        {
            VerticalAlignment = VerticalAlignment.Center;
            Orientation = Orientation.Horizontal;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Loaded += HyperlinkLabel_Loaded;
            }
        }

    }
}
