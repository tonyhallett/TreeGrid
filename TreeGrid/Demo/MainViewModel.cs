using System;

namespace TreeGrid
{
    public class MainViewModel
    {
        public TreeGridViewModel ViewModel { get; } = new TreeGridViewModel();

        internal void Update() => ViewModel.Update();
    }
}
