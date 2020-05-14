namespace AtomosZ.OhBehave.EditorTools
{
	public class ConnectionControls
	{
		private static ConnectionPoint selectedInPoint;
		private static ConnectionPoint selectedOutPoint;

		internal static void OnClickInPoint(ConnectionPoint inPoint)
		{
			selectedInPoint = inPoint;
			if (selectedOutPoint != null)
			{
				if (selectedOutPoint.node != selectedInPoint.node)
				{
					CreateConnection();
					ClearConnectionSelection();
				}
				else
				{
					ClearConnectionSelection();
				}
			}
		}

		internal static void OnClickOutPoint(ConnectionPoint outPoint)
		{
			selectedOutPoint = outPoint;

			if (selectedInPoint != null)
			{
				if (selectedOutPoint.node != selectedInPoint.node)
				{
					CreateConnection();
					ClearConnectionSelection();
				}
				else
				{
					ClearConnectionSelection();
				}
			}
		}

		private static void CreateConnection()
		{
			((CompositeNodeWindow)selectedOutPoint.node).CreateChildConnection(selectedInPoint.node);
		}

		private static void ClearConnectionSelection()
		{
			selectedInPoint = null;
			selectedOutPoint = null;
		}
	}
}