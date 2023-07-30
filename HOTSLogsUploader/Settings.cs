using System.Linq;

namespace HOTSLogsUploader.Properties
{
    internal sealed partial class Settings
    {        
        public Settings()
        {
            
        }

         public override void Upgrade()
        {
            base.Upgrade();
            Reset();
            Save();
        }
    }
}
