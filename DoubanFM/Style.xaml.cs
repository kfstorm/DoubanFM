using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DoubanFM
{
	public partial class Style : ResourceDictionary
	{

		private void Link_Click(object sender, RoutedEventArgs e)
		{
			// 在此处添加事件处理程序实现。
			Process.Start((string)((Button)sender).Tag);
		}

		private void WindowBaseStyle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			//如果不加try catch语句，在点击封面打开资料页面时很容易报错
			try
			{
				(sender as Window).DragMove();
			}
			catch { }
		}

		private void ChildWindowBaseStyle_ButtonClose_Click(object sender, RoutedEventArgs e)
		{
			((sender as FrameworkElement).TemplatedParent as Window).Close();
		}

		private Random random = new Random();

		private void ChannelStyle_Loaded(object sender, RoutedEventArgs e)
		{
			ListBoxItem item = sender as ListBoxItem;
			Storyboard storyboard = new Storyboard();

			DoubleAnimationUsingKeyFrames opacityFrames = new DoubleAnimationUsingKeyFrames();
			opacityFrames.KeyFrames.Add(new DiscreteDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
			KeyTime time = KeyTime.FromTimeSpan(new TimeSpan((long)((random.NextDouble() / 2 + 0.5) * 10000000)));
			opacityFrames.KeyFrames.Add(new LinearDoubleKeyFrame(1, time));
			storyboard.Children.Add(opacityFrames);

			item.RenderTransform = new TranslateTransform();
			DoubleAnimationUsingKeyFrames OffsetXFrames = new DoubleAnimationUsingKeyFrames();
			OffsetXFrames.KeyFrames.Add(new DiscreteDoubleKeyFrame(-50, KeyTime.FromTimeSpan(TimeSpan.Zero)));
			OffsetXFrames.KeyFrames.Add(new EasingDoubleKeyFrame(0, time, new CircleEase()));
			storyboard.Children.Add(OffsetXFrames);

			Storyboard.SetTarget(opacityFrames, item);
			Storyboard.SetTargetProperty(opacityFrames, new PropertyPath("Opacity"));
			Storyboard.SetTarget(OffsetXFrames, item);
			Storyboard.SetTargetProperty(OffsetXFrames, new PropertyPath("RenderTransform.(TranslateTransform.X)"));
		
			storyboard.Begin();
		}

		private void ControlPanelStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.OriginalSource == sender)
			{
				((sender as FrameworkElement).Style.Resources["ControlPanelStoryboard"] as Storyboard).Begin(sender as FrameworkElement);
			}
		}
	}
}
