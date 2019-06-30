using System.Collections.Generic;
using System.Text;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;

namespace Rocket.Surgery.Cake
{

    /// <summary>
    /// Class CommonCakeAliases.
    /// </summary>
    public static class CommonCakeAliases
    {
        private static readonly string ByteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

        /// <summary>
        /// Cleans the bom.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="file">The file.</param>
        [CakeMethodAlias]
        public static void CleanBom(this ICakeContext context, FilePath file)
        {
            var withBom = System.IO.File.ReadAllText(file.FullPath);
            System.IO.File.WriteAllText(file.FullPath, withBom.Replace(ByteOrderMarkUtf8, ""));
        }

        /// <summary>
        /// Cleans the bom.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="filePath">The file path.</param>
        [CakeMethodAlias]
        public static void CleanBom(this ICakeContext context, string filePath)
        {
            CleanBom(context, FilePath.FromString(filePath));
        }
    }
}
