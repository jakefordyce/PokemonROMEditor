using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonROMEditor.Models
{
    public class ROMBank
    {        
        public ROMBank()
        {
            StartByte = 0;
            EndByte = 0x3fff;
            UsedDataBlocks = new List<UsedDataBlock>();
        }
        public ROMBank(int start, int end)
        {
            StartByte = start;
            EndByte = end;
            UsedDataBlocks = new List<UsedDataBlock>();
        }
        public int EndByte { get; set; }
        public int StartByte { get; set; }
        public List<UsedDataBlock> UsedDataBlocks { get;}
        
        /// <summary>
        /// Mark a space in the bank as used.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public int AddDataBlock(int start, int end)
        {
            if(end > EndByte || start < StartByte)
            {
                return -1;
            }
            else
            {
                var block = new UsedDataBlock();
                block.StartByte = start;
                block.EndByte = end;

                UsedDataBlocks.Add(block);
                UsedDataBlocks.Sort((x, y) => x.StartByte.CompareTo(y.StartByte));

                return UsedDataBlocks.IndexOf(block);
            }
        }

        public void AddData(int start, int size)
        {
            if(UsedDataBlocks.Count() > 0)
            {
                bool needsNewBlock = true;
                //check ours current blocks to see if we can extend one that is already there.
                for(int i = 0; i < UsedDataBlocks.Count(); i++)
                {
                    if(UsedDataBlocks.ElementAt(i).EndByte == start - 1)
                    {
                        needsNewBlock = false;
                        ExtendBlock(i, size);
                        break;
                    }
                }
                //otherwise add a new one
                if (needsNewBlock)
                {
                    AddDataBlock(start, start + size);
                }
            }
            else
            {
                //if there's no current blocks start a new one.
                AddDataBlock(start, start + size);
            }
        }

        public void ExtendBlock(int blockIndex, int size)
        {
            UsedDataBlocks.ElementAt(blockIndex).EndByte += size;
        }
        
        /// <summary>
        /// Pass in the number of bytes you want to save and it will tell you the lowest address that has free space for the bytes.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public int HasRoomAt(int size)
        {
            if(size > (EndByte - StartByte)) // make sure the data will fit into the bank
            {
                return -1;
            }
            else
            {
                if(UsedDataBlocks.Count == 0) // if it fits in the bank and there are no blocks then we know we have enough room.
                {
                    return StartByte;
                }
                else
                {
                    if (UsedDataBlocks.ElementAt(0).Contains(StartByte + size))
                    {
                        for(int i = 0; i < UsedDataBlocks.Count(); i++)
                        {
                            if(i + 1 == UsedDataBlocks.Count())//last block, compare to end byte
                            {
                                if(EndByte - UsedDataBlocks.ElementAt(i).EndByte >= size)
                                {
                                    return UsedDataBlocks.ElementAt(i).EndByte + 1;
                                }
                                else
                                {
                                    return -2;
                                }
                            }
                            else
                            {
                                if(UsedDataBlocks.ElementAt(i+1).StartByte - UsedDataBlocks.ElementAt(i).EndByte - 1 >= size) //if there's enough room between the current block and the next block
                                {
                                    return UsedDataBlocks.ElementAt(i).EndByte + 1;
                                }
                            }
                            //check each block against its next block or against the end
                        }
                    }
                    else // the first block didn't contain the address which means there is room at the start of the block.
                    {
                        return StartByte;
                    }
                }
            }
            return -2;
        }
    }

    public class UsedDataBlock
    {
        public int StartByte { get; set; }
        public int EndByte { get; set; }
        public bool Contains(int address)
        {
            return (StartByte < address && EndByte > address);
        }
    }
}
