using System;

namespace BlazorBlogsLibrary.Classes
{
    public class DisqusState
    {
        private bool _DisplayDisqus;

        // StateChanged is an event handler other pages
        // can subscribe to 
        public event EventHandler StateChanged;

        public bool getDisplayDisqus()
        {
            return _DisplayDisqus;
        }

        public void SetDisplayDisqus(bool param)
        {
            _DisplayDisqus = param;
            StateHasChanged();
        }

        private void StateHasChanged()
        {
            // This will update any subscribers
            // that the counter state has changed
            // so they can update themselves
            // and show the current counter value
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}