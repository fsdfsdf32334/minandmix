using System.Windows.Forms;

namespace LOVESIX;

internal sealed class AppContext : ApplicationContext
{
	public AppContext()
	{
		ShowLogin();
	}

	private void ShowLogin()
	{
		LoginForm login = new LoginForm();
		login.FormClosed += delegate
		{
			if (login.Succeeded)
			{
				ShowMain();
			}
			else
			{
				ExitThread();
			}
		};
		login.Show();
	}

	private void ShowMain()
	{
		MainForm mainForm = new MainForm();
		mainForm.LogoutRequested += ShowLogin;
		mainForm.Show();
	}
}
