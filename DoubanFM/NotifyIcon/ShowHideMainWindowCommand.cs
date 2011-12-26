using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace DoubanFM.NotifyIcon
{
	public class ShowHideMainWindowCommand : ICommand
	{
		public bool CanExecute(object parameter)
		{
			return true;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			DoubanFMWindow window = App.Current.MainWindow as DoubanFMWindow;
			if (window != null && window.IsLoaded)
			{
				if (window.IsVisible == false)
					window.ShowFront();
				else
					window.Hide();
			}
		}
	}
}
