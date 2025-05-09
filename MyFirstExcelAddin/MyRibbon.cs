using Microsoft.Office.Tools.Ribbon;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms; // Required for MessageBox
using System.Diagnostics; // Required for Debug.WriteLine

namespace MyFirstExcelAddin
{
    public partial class MyRibbon
    {
        // Dictionary to store original values: Key = Cell Address (e.g., "Sheet1!A1"), Value = Original Value
        private Dictionary<string, object> originalValues = new Dictionary<string, object>();

        private void MyRibbon_Load(object sender, RibbonUIEventArgs e)
        {
        }

        /// <summary>
        /// Handles the click event for the "Convert to Alphanumeric" button.
        /// Sanitizes the selected cell range by removing non-alphanumeric characters.
        /// Stores original values for reverting.
        /// </summary>
        private void btnConvertToAlphanumeric_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Range selectedRange = null;
            try
            {
                // Get the currently selected range
                selectedRange = Globals.ThisAddIn.Application.Selection as Excel.Range;

                // Check if a range is actually selected
                if (selectedRange == null)
                {
                    MessageBox.Show("Please select a range of cells.", "No Range Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Clear previous original values before processing a new range
                originalValues.Clear();

                // Iterate through each cell in the selected range
                foreach (Excel.Range cell in selectedRange.Cells)
                {
                    // Get the cell's address (including sheet name) to use as a unique key
                    string cellAddress = cell.get_Address(false, false, Excel.XlReferenceStyle.xlA1, true);

                    // Store the original value before modifying it
                    originalValues[cellAddress] = cell.Value2; // Use Value2 to get raw value (number, string, error, etc.)

                    // Get the cell value as a string for processing
                    // Use cell.Text to get the displayed string value, handle potential nulls
                    string cellValue = cell.Text != null ? cell.Text.ToString() : string.Empty;

                    // Sanitize the string: remove all characters that are NOT alphanumeric (a-z, A-Z, 0-9)
                    // Regex pattern [^a-zA-Z0-9] matches any character that is NOT in the set
                    string sanitizedValue = Regex.Replace(cellValue, "[^a-zA-Z0-9]", "");

                    // Update the cell value with the sanitized string
                    cell.Value2 = sanitizedValue;
                }

                MessageBox.Show("Selected cells converted to alphanumeric.", "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (System.Exception ex)
            {
                // Handle any errors that occur during the process
                // Display the detailed error message
                MessageBox.Show($"An error occurred during conversion: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Conversion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Conversion Error: {ex.Message}\nStack Trace:\n{ex.StackTrace}"); // Also write to output window
            }
            finally
            {
                // Release COM objects to prevent memory leaks
                if (selectedRange != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(selectedRange);
            }
        }

        /// <summary>
        /// Handles the click event for the "Revert to Original" button.
        /// Restores the original values of the cells that were previously converted.
        /// </summary>
        private void btnRevertToOriginal_Click(object sender, RibbonControlEventArgs e)
        {
            Excel.Range cell = null; // Declare outside the loop for finally block
            try
            {
                // Check if there are any stored original values to revert
                if (originalValues.Count == 0)
                {
                    MessageBox.Show("No values to revert. Please use 'Convert to Alphanumeric' first.", "Nothing to Revert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Iterate through the stored original values
                foreach (KeyValuePair<string, object> entry in originalValues)
                {
                    string cellAddress = entry.Key;
                    object originalValue = entry.Value;

                    // Get the Excel Range object for the stored address
                    // Need to get the Application object from the Add-in
                    // Use try-catch within the loop for cell-specific errors during reversion
                    try
                    {
                        cell = Globals.ThisAddIn.Application.get_Range(cellAddress);

                        // Restore the original value to the cell
                        cell.Value2 = originalValue;
                    }
                    catch (System.Exception cellEx)
                    {
                        // Report error for the specific cell
                        MessageBox.Show($"Error reverting cell {cellAddress}: {cellEx.Message}", "Reversion Error (Cell Specific)", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Debug.WriteLine($"Reversion Error for cell {cellAddress}: {cellEx.Message}\nStack Trace:\n{cellEx.StackTrace}"); // Also write to output window
                    }
                    finally
                    {
                        // Release the COM object for the cell range inside the loop
                        if (cell != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(cell);
                        cell = null; // Set to null to avoid releasing an invalid object in finally if loop breaks
                    }
                }

                // Clear the stored original values after reverting
                originalValues.Clear();

                MessageBox.Show("Cells reverted to original values.", "Reversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            catch (System.Exception ex)
            {
                // Handle any errors that occur during the process
                MessageBox.Show($"An error occurred during reversion: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Reversion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Reversion Error: {ex.Message}\nStack Trace:\n{ex.StackTrace}"); // Also write to output window
            }
            finally
            {
                // Ensure the last cell COM object is released if the loop completed or an error occurred
                if (cell != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(cell);
            }
        }
    }
}
