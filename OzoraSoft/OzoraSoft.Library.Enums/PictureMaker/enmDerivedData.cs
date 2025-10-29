namespace OzoraSoft.Library.Enums.PictureMaker
{
    /// <summary>
    /// 
    /// </summary>
    public enum enmDerivedData
    {
        #region 1. Dynamic:
        /// <summary>
        /// 
        /// </summary>
        dyna_map,
        /// <summary>
        /// 
        /// </summary>
        dyna_raw,
        /// <summary>
        /// 
        /// </summary>
        dyna_raw_mean,
        /// <summary>
        /// 
        /// </summary>
        dyna_raw_std,
        /// <summary>
        /// 
        /// </summary>
        dyna_mean,
        /// <summary>
        /// 
        /// </summary>
        dyna_std,
        /// <summary>
        /// 
        /// </summary>
        dyna_ncp,
        /// <summary>
        /// 
        /// </summary>
        dyna_clipped,
        /// <summary>
        /// 
        /// </summary>
        dyna_clipped_mean,
        /// <summary>
        /// 
        /// </summary>
        dyna_clipped_std,
        /// <summary>
        /// 
        /// </summary>
		dyna_clipped_ncp,
        /// <summary>
        /// 
        /// </summary>
        dyna_hclc_ncp,
        /// <summary>
        /// 
        /// </summary>
        mean_sum_cc,
        /// <summary>
        /// 
        /// </summary>
        ocr_map,
        #endregion

        #region 2. Electrical:
        /// <summary>
        /// 
        /// </summary>
        electricalOffset,
        /// <summary>
        /// 
        /// </summary>
        electricalTrimFe,
        /// <summary>
        /// 
        /// </summary>
        electricalTrimBe,
        /// <summary>
        /// 
        /// </summary>
        electricalTrimBe2,
        /// <summary>
        /// 
        /// </summary>
        electricalLkgActive,
        /// <summary>
        /// 
        /// </summary>
        electricalAddCap,
        /// <summary>
        /// 
        /// </summary>
        electricalQTrig,
        #endregion

        #region 3. Energy_Cal / 5. Spectrum:
        /// <summary>
        /// 
        /// </summary>
        spec_mean,
        /// <summary>
        /// 
        /// </summary>
        spec_median,
        /// <summary>
        /// 
        /// </summary>
        spec_pixel,
        /// <summary>
        /// 
        /// </summary>
        spec_smoothed,
        /// <summary>
        /// 
        /// </summary>
        am241_fwhm,
        /// <summary>
        /// 
        /// </summary>
        am241_fwhm_mean,
        /// <summary>
        /// 
        /// </summary>
        am241_fwhm_sigma,
        /// <summary>
        /// 
        /// </summary>
        am241_fwhm_ncps,
        /// <summary>
        /// 
        /// </summary>
        am241_peak,
        /// <summary>
        /// 
        /// </summary>
        am241_peak_ncps,
        #endregion

        #region 4. Uniformity:
        /// <summary>
        /// 
        /// </summary>
        ocr_Q_fence_low,
        /// <summary>
        /// 
        /// </summary>
        ocr_Q_fence_high,
        /// <summary>
        /// 
        /// </summary>
        ocr_uniformity,
        /// <summary>
        /// 
        /// </summary>
        ocr_total_counts,
        /// <summary>
        /// 
        /// </summary>
        ocr_corrected,
        /// <summary>
        /// 
        /// </summary>
        ocr_cleaned,
        /// <summary>
        /// 
        /// </summary>
        ocr_mean,
        /// <summary>
        /// 
        /// </summary>
        ocr_median,
        /// <summary>
        /// 
        /// </summary>
        ocr_std,
        /// <summary>
        /// 
        /// </summary>
        ocr_raw_sum_cc,
        /// <summary>
        /// 
        /// </summary>
        ocr_raw_open,
        /// <summary>
        /// 
        /// </summary>
        ocr_raw_mean,
        /// <summary>
        /// 
        /// </summary>
        ocr_icr_comp,
        /// <summary>
        /// 
        /// </summary>
        ocr_icr_ncp,
        /// <summary>
        /// 
        /// </summary>
        ocr_icr_mean,
        /// <summary>
        /// 
        /// </summary>
        ocr_icr_std,
        /// <summary>
        /// 
        /// </summary>
        ocr_ncp,
        /// <summary>
        /// 
        /// </summary>
        ocr_outliers,
        /// <summary>
        /// 
        /// </summary>
        ocr_outlier_count,
        /// <summary>
        /// 
        /// </summary>
        ocr_percent_of_mean,
        /// <summary>
        /// 
        /// </summary>
        counting_ncps,
        /// <summary>
        /// 
        /// </summary>
        counting_beta,
        /// <summary>
        /// 
        /// </summary>
        counting_coeffs,
        /// <summary>
        /// 
        /// </summary>
        raw_sum,
        /// <summary>
        /// 
        /// </summary>
        raw_sum_resampled,
        #endregion

        #region 6. Stability:      
        /// <summary>
        /// 
        /// </summary>
        d_number,
        /// <summary>
        /// 
        /// </summary>
        d_number_ncps,
        /// <summary>
        /// 
        /// </summary>
		norm,
        /// <summary>
        /// 
        /// </summary>
		norm_mask,
        /// <summary>
        /// 
        /// </summary>
		norm_sum,
        /// <summary>
        /// 
        /// </summary>
        md1f,
        /// <summary>
        /// 
        /// </summary>
        md1f_ncps,
        /// <summary>
        /// 
        /// </summary>
        md1s,
        /// <summary>
        /// 
        /// </summary>
        md1s_ncps,
        #endregion

        #region 7. Energy:
        /// <summary>
        /// 
        /// </summary>
        energyGain0,
        /// <summary>
        /// 
        /// </summary>
        energyGain1,
        /// <summary>
        /// 
        /// </summary>
        energyOffset0,
        /// <summary>
        /// 
        /// </summary>
        energyOffset1,
        /// <summary>
        /// 
        /// </summary>
        energyNcp0,
        /// <summary>
        /// 
        /// </summary>
        energyNcp1,
        #endregion

        #region 8. NCPs
        /// <summary>
        /// 
        /// </summary>
        ncp_combined,
        /// <summary>
        /// 
        /// </summary>
		ncp_pixels
        #endregion

    }
}
