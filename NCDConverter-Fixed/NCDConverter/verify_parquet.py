"""
NCD Converter Output Verification Script
Tests that converted Parquet files are valid and contain expected data
"""

import pandas as pd
import pyarrow.parquet as pq
import os
import sys
from pathlib import Path

def test_parquet_file(file_path):
    """
    Verify a Parquet file has the correct structure and data
    
    Args:
        file_path: Path to .parquet file
        
    Returns:
        True if valid, False otherwise
    """
    print(f"\n{'='*60}")
    print(f"Testing: {os.path.basename(file_path)}")
    print(f"{'='*60}")
    
    try:
        # Read the Parquet file
        df = pd.read_parquet(file_path)
        
        # Basic checks
        print(f"✓ File loaded successfully")
        print(f"  Records: {len(df):,}")
        print(f"  Columns: {list(df.columns)}")
        print(f"  Memory: {df.memory_usage(deep=True).sum() / 1024 / 1024:.2f} MB")
        
        # Check for required columns
        if 'timestamp' in df.columns:
            print(f"✓ Timestamp column present")
            df['timestamp'] = pd.to_datetime(df['timestamp'])
            print(f"  Date range: {df['timestamp'].min()} to {df['timestamp'].max()}")
            duration = df['timestamp'].max() - df['timestamp'].min()
            print(f"  Duration: {duration}")
        else:
            print(f"✗ Missing timestamp column")
            return False
            
        # Check data type (Minute or Tick)
        if 'open' in df.columns and 'high' in df.columns:
            print(f"✓ Minute data detected (OHLC)")
            print(f"  Open range: ${df['open'].min():.2f} - ${df['open'].max():.2f}")
            print(f"  High: ${df['high'].max():.2f}")
            print(f"  Low: ${df['low'].min():.2f}")
            print(f"  Close range: ${df['close'].min():.2f} - ${df['close'].max():.2f}")
            print(f"  Total volume: {df['volume'].sum():,.0f}")
        elif 'price' in df.columns:
            print(f"✓ Tick data detected")
            print(f"  Price range: ${df['price'].min():.2f} - ${df['price'].max():.2f}")
            if 'bid' in df.columns:
                print(f"  Bid range: ${df['bid'].min():.2f} - ${df['bid'].max():.2f}")
            if 'ask' in df.columns:
                print(f"  Ask range: ${df['ask'].min():.2f} - ${df['ask'].max():.2f}")
            print(f"  Total volume: {df['volume'].sum():,.0f}")
        else:
            print(f"✗ Unknown data format")
            return False
            
        # Check for nulls
        null_counts = df.isnull().sum()
        if null_counts.sum() > 0:
            print(f"⚠ Warning: Found null values:")
            for col, count in null_counts[null_counts > 0].items():
                print(f"    {col}: {count} nulls")
        else:
            print(f"✓ No null values")
            
        # Check for duplicates
        duplicates = df.duplicated().sum()
        if duplicates > 0:
            print(f"⚠ Warning: {duplicates} duplicate rows")
        else:
            print(f"✓ No duplicate rows")
            
        # Data quality checks
        if 'volume' in df.columns:
            if (df['volume'] < 0).any():
                print(f"✗ ERROR: Negative volumes detected")
                return False
            print(f"✓ All volumes are non-negative")
            
        print(f"\n✓ File validation PASSED")
        return True
        
    except Exception as e:
        print(f"✗ ERROR: {e}")
        import traceback
        traceback.print_exc()
        return False

def test_directory(directory):
    """Test all Parquet files in a directory"""
    print(f"\n{'#'*60}")
    print(f"# Testing Parquet files in: {directory}")
    print(f"{'#'*60}")
    
    parquet_files = list(Path(directory).glob("*.parquet"))
    
    if not parquet_files:
        print(f"\n✗ No Parquet files found in {directory}")
        return
        
    print(f"\nFound {len(parquet_files)} Parquet file(s)")
    
    results = {}
    for file_path in parquet_files:
        results[file_path.name] = test_parquet_file(str(file_path))
        
    # Summary
    print(f"\n{'='*60}")
    print(f"SUMMARY")
    print(f"{'='*60}")
    passed = sum(1 for v in results.values() if v)
    failed = len(results) - passed
    print(f"Total files: {len(results)}")
    print(f"Passed: {passed} ✓")
    print(f"Failed: {failed} ✗")
    
    if failed == 0:
        print(f"\n🎉 All files validated successfully!")
    else:
        print(f"\n⚠ Some files failed validation")
        
    return failed == 0

def main():
    """Main entry point"""
    if len(sys.argv) < 2:
        print("Usage: python verify_parquet.py <directory_or_file>")
        print("\nExamples:")
        print("  python verify_parquet.py C:\\output")
        print("  python verify_parquet.py C:\\output\\20241021.parquet")
        sys.exit(1)
        
    path = sys.argv[1]
    
    if not os.path.exists(path):
        print(f"✗ Path does not exist: {path}")
        sys.exit(1)
        
    if os.path.isfile(path):
        # Test single file
        success = test_parquet_file(path)
        sys.exit(0 if success else 1)
    elif os.path.isdir(path):
        # Test directory
        success = test_directory(path)
        sys.exit(0 if success else 1)
    else:
        print(f"✗ Invalid path: {path}")
        sys.exit(1)

if __name__ == "__main__":
    main()
