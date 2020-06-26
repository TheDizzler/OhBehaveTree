namespace AtomosZ.OhBehave.EditorTools
{
	public interface IParentNodeWindow
	{
		void CreateChildConnection(NodeWindow newNode);
		void RemoveChildConnection(NodeWindow removeNode);
		string GetName();
	}
}