using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Paradox.Shaders.Parser.Ast
{
    public class MemberName : TypeBase, IDeclaration, IScopeContainer, IGenericStringArgument
    {
        #region Constructors and Destructors
        /// <summary>
        ///   Initializes a new instance of the <see cref = "MemberName" /> class.
        /// </summary>
        public MemberName()
            : base("MemberName")
        {
        }

        #endregion
    }
}