namespace DotRLGlueCodec.TaskSpec
{
    public interface ITaskSpecParser
    {
        TaskSpecBase Parse(string taskSpecString);
    }
}
