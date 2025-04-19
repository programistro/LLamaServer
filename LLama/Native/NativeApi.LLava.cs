using System;

namespace LLama.Native;

public static partial class NativeApi
{
    /// <summary>
    /// Sanity check for clip &lt;-&gt; llava embed size match
    /// </summary>
    /// <param name="ctxLlama">LLama Context</param>
    /// <param name="ctxClip">Llava Model</param>
    /// <returns>True if validate successfully</returns>
    [DllImport(llavaLibraryName, EntryPoint = "llava_validate_embed_size", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool llava_validate_embed_size( SafeLLamaContextHandle ctxLlama, SafeLlavaModelHandle ctxClip);

    /// <summary>
    /// Build an image embed from image file bytes
    /// </summary>
    /// <param name="ctx_clip">SafeHandle to the Clip Model</param>
    /// <param name="n_threads">Number of threads</param>
    /// <param name="image_bytes">Binary image in jpeg format</param>
    /// <param name="image_bytes_length">Bytes length of the image</param>
    /// <returns>SafeHandle to the Embeddings</returns>
    [DllImport(llavaLibraryName, EntryPoint = "llava_image_embed_make_with_bytes",
        CallingConvention = CallingConvention.Cdecl)]
    public static extern 
        SafeLlavaImageEmbedHandle llava_image_embed_make_with_bytes(SafeLlavaModelHandle ctx_clip, int n_threads,
                                                                    byte[] image_bytes, int image_bytes_length);

    /// <summary>
    /// Build an image embed from a path to an image filename
    /// </summary>
    /// <param name="ctx_clip">SafeHandle to the Clip Model</param>
    /// <param name="n_threads">Number of threads</param>
    /// <param name="image_path">Image filename (jpeg) to generate embeddings</param>
    /// <returns>SafeHandle to the embeddings</returns>
    [DllImport(llavaLibraryName, EntryPoint = "llava_image_embed_make_with_filename", CallingConvention = CallingConvention.Cdecl)]
    public static extern 
        SafeLlavaImageEmbedHandle llava_image_embed_make_with_filename(SafeLlavaModelHandle ctx_clip, int n_threads,
                                                                       [MarshalAs(UnmanagedType.LPStr)] string image_path);

    /// <summary>
    /// Free an embedding made with llava_image_embed_make_*
    /// </summary>
    /// <param name="embed">Embeddings to release</param>
    [DllImport(llavaLibraryName, EntryPoint = "llava_image_embed_free", CallingConvention = CallingConvention.Cdecl)]
    public static extern void llava_image_embed_free(IntPtr embed);

    /// <summary>
    /// Write the image represented by embed into the llama context with batch size n_batch, starting at context
    /// pos n_past. on completion, n_past points to the next position in the context after the image embed.
    /// </summary>
    /// <param name="ctx_llama">Llama Context</param>
    /// <param name="embed">Embedding handle</param>
    /// <param name="n_batch"></param>
    /// <param name="n_past"></param>
    /// <returns>True on success</returns>
    [DllImport(llavaLibraryName, EntryPoint = "llava_eval_image_embed", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool llava_eval_image_embed(SafeLLamaContextHandle ctx_llama, SafeLlavaImageEmbedHandle embed, int n_batch, ref int n_past);
    
}