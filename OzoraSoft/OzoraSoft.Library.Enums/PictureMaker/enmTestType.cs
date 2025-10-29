namespace OzoraSoft.Library.Enums.PictureMaker
{
    /// <summary>
    /// 
    /// </summary>
    public enum enmTestType : int
    {
        /// <summary>
        /// Error if null
        /// </summary>
        none = -1,
        /// <summary>
        /// Dynamic response test (aka 1 second stability) 
        /// Modules: Sensor,MM,DM
        /// </summary>
        dynamic = 51,
        /// <summary>
        /// 
        /// </summary>
        electrical = 52,
        /// <summary>
        /// k-edge energy calibration 
        /// Modules: MM,DM
        /// </summary>
        energy_cal = 53,
        /// <summary>
        /// Uniformity test 
        /// Modules: MM/DM
        /// </summary>
        uniformity = 54,
        /// <summary>
        /// Spectral scan
        ///  Modules: MM,DM
        /// </summary>
        spectrum = 55,
        /// <summary>
        /// Short term stability test (aka 1 min stability) 
        /// Modules: MM,DM
        /// </summary>
        stability = 56,
        /// <summary>
        /// 
        /// </summary>
        energy = 57,
        /// <summary>
        /// 
        /// </summary>
        ncps = 58,
        /// <summary>
        /// 
        /// </summary>
		HVCurrentLimit = 99,
        /// <summary>
        /// 
        /// </summary>
        heatmapDM=100
    }
}