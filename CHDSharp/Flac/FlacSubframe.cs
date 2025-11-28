using CHDReaderTest.Flac.FlacDeps;

namespace CUETools.Codecs.Flake
{
    public class FlacSubframe
    {
        public FlacSubframe()
        {
            rc = new RiceContext();
            coefs = new int[lpc.MAX_LPC_ORDER];
            residual = new int[FlakeConstants.MAX_BLOCKSIZE];
        }
        public SubframeType type;
        public int order;
        public int[] residual;
        public int residualOffset;
        public RiceContext rc;
        public uint size;

        public int cbits;
        public int shift;
        public int[] coefs;
        public int window;
    };
}
