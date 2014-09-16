﻿//2014 Apache2, WinterDev
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using LayoutFarm;

namespace LayoutFarm.Grids
{

    public sealed class GridLayer : VisualLayer
    {
        GridRowCollection gridRows;
        GridColumnCollection gridCols;

        int uniformCellWidth;
        int uniformCellHeight;
        CellSizeStyle flexgridType;
        public GridLayer(RenderElement owner, int nColumns, int nRows, CellSizeStyle flexgridType)
            : base(owner)
        {
            this.flexgridType = flexgridType;
            gridRows = new GridRowCollection(this);
            gridCols = new GridColumnCollection(this);


            int columnWidth = owner.Width;
            if (nColumns > 0)
            {
                columnWidth = columnWidth / nColumns;
                uniformCellWidth = columnWidth;
                if (columnWidth < 1)
                {
                    columnWidth = 1;
                }
            }
            //------------------------------------------------------------             
            for (int c = 0; c < nColumns; c++)
            {
                gridCols.Add(new GridColumn(columnWidth));
            }
            //------------------------------------------------------------
            if (nRows > 0)
            {
                int rowHeight = owner.Height / nRows;
                int cy = 0;
                for (int r = 0; r < nRows; r++)
                {
                    var rowDef = new GridRow(rowHeight);
                    rowDef.Top = cy;
                    gridRows.Add(rowDef);
                    cy += rowHeight;
                }
                uniformCellHeight = rowHeight;
            }
        }
        public override bool HitTestCore(HitPointChain artHitResult)
        {
            int testX;
            int testY;
            artHitResult.GetTestPoint(out testX, out testY);
            GridCell gridItem = GetGridItemByPosition(testX, testY);

            if (gridItem != null && gridItem.ContentElement != null)
            {

                artHitResult.OffsetTestPoint(-gridItem.X, -gridItem.Y);
                //  2010-09-04 ?
                gridItem.ContentElement.HitTestCore(artHitResult);

                artHitResult.OffsetTestPoint(gridItem.X, gridItem.Y);
                return true;
            }
            return false;
        }
        public override void Clear()
        {

            gridCols = new GridColumnCollection(this);
            gridRows = new GridRowCollection(this);
        }
        public int RowCount
        {
            get
            {
                return gridRows.Count;
            }
        }

        public override void TopDownReArrangeContent()
        {

#if DEBUG
            vinv_dbug_EnterLayerReArrangeContent(this);
#endif
            //--------------------------------- 
            this.BeginLayerLayoutUpdate();
            //---------------------------------
            if (gridCols != null && gridCols.Count > 0)
            {


                int curY = 0;
                foreach (GridRow rowDef in gridRows.RowIter)
                {

                    rowDef.AcceptDesiredHeight(curY);
                    curY += rowDef.Height;
                }

                int curX = 0;
                foreach (GridColumn gridCol in gridCols.GetColumnIter())
                {

                    gridCol.SetLeftAndPerformArrange(curX);
                    curX += gridCol.Width;
                }
            }

            ValidateArrangement();
            //---------------------------------
            this.EndLayerLayoutUpdate();

#if DEBUG
            vinv_dbug_ExitLayerReArrangeContent();
#endif
        }

        public override IEnumerable<RenderElement> GetVisualElementIter()
        {

            if (gridCols != null && gridCols.Count > 0)
            {
                foreach (GridColumn gridCol in gridCols.GetColumnIter())
                {

                    foreach (RenderElement ve in gridCol.GetTopDownVisualElementIter())
                    {
                        yield return ve;
                    }
                }
            }
        }
        public override IEnumerable<RenderElement> GetVisualElementReverseIter()
        {
            if (gridCols != null && gridCols.Count > 0)
            {
                foreach (GridColumn gridCol in gridCols.GetColumnReverseIter())
                {
                    foreach (RenderElement ve in gridCol.GetTopDownVisualElementIter())
                    {
                        yield return ve;
                    }
                }
            }
        }


        public void ChangeColumnWidth(GridColumn targetGridColumn, int newWidth)
        {

            targetGridColumn.Width = newWidth;

            GridColumn prevColumn = targetGridColumn;
            GridColumn currentColumn = targetGridColumn.NextColumn;
            while (currentColumn != null)
            {
                currentColumn.Left = prevColumn.Right;
                prevColumn = currentColumn;
                currentColumn = currentColumn.NextColumn;
            }
            owner.InvalidateLayoutAndStartBubbleUp();
        }
        public int UniformCellWidth
        {
            get
            {
                return uniformCellWidth;
            }
        }

        public int UniformCellHeight
        {
            get
            {
                return uniformCellHeight;
            }
        }

        public CellSizeStyle GridType
        {
            get
            {
                return flexgridType;

            }
        }

        public GridCell GetGridItemByPosition(int x, int y)
        {
            if (y < 0)
            {
                y = 0;
            }
            if (x < 0)
            {
                x = 0;
            }

            switch (flexgridType)
            {
                case CellSizeStyle.UniformWidth:
                    {
                        GridRow row = gridRows.GetRowAtPos(y);
                        if (row != null)
                        {

                            int columnNumber = x / uniformCellWidth;
                            if (columnNumber >= gridCols.Count)
                            {
                                columnNumber = gridCols.Count - 1;
                            }

                            GridColumn column = gridCols[columnNumber];
                            if (column == null)
                            {
                                column = gridCols.Last;
                            }
                            if (column != null)
                            {
                                return column.GetCell(row.RowIndex);
                            }
                        }
                    } break;
                case CellSizeStyle.UniformHeight:
                    {

                        int rowNumber = y / uniformCellHeight;
                        if (rowNumber >= gridRows.Count)
                        {
                            rowNumber = gridRows.Count - 1;
                        }
                        GridRow row = gridRows[rowNumber];
                        if (row != null)
                        {
                            GridColumn column = gridCols.GetColumnAtPosition(x);
                            if (column == null)
                            {
                                column = gridCols.Last;
                            }
                            if (column != null)
                            {
                                return column.GetCell(row.RowIndex);
                            }
                        }
                    } break;
                case CellSizeStyle.UniformCell:
                    {

                        int rowNumber = y / uniformCellHeight;
                        if (rowNumber >= gridRows.Count)
                        {
                            rowNumber = gridRows.Count - 1;
                        }
                        GridRow row = gridRows[rowNumber];

                        if (row != null)
                        {

                            int columnNumber = x / uniformCellWidth;

                            if (columnNumber >= gridCols.Count)
                            {
                                columnNumber = gridCols.Count - 1;
                            }
                            GridColumn column = gridCols[columnNumber];

                            if (column == null)
                            {
                                column = gridCols.Last;
                            }
                            if (column != null)
                            {
                                return column.GetCell(row.RowIndex);
                            }
                        }

                    } break;
                default:
                    {
                        GridRow row = gridRows.GetRowAtPos(y);
                        if (row == null)
                        {

                            row = gridRows.Last;
                        }
                        if (row != null)
                        {
                            GridColumn column = gridCols.GetColumnAtPosition(x);

                            if (column == null)
                            {
                                column = gridCols.Last;
                            }
                            if (column != null)
                            {
                                return column.GetCell(row.RowIndex);
                            }
                        }
                    } break;
            }
            return null;
        }
        public GridCell GetCell(int rowIndex, int columnIndex)
        {

            return gridCols[columnIndex].GetCell(rowIndex);
        }

        public void AdjustGridWidth(int nWidthDiff)
        {


            int j = gridCols.Count;
            if (j > 0)
            {
                //just average
                int avgWidth = nWidthDiff / j;
                if (avgWidth > 0)
                {
                    for (int i = j - 1; i > -1; i--)
                    {

                        gridCols[i].Width += avgWidth;

                    }
                }
            }
        }


        public void SetUniformGridItemSize(int cellItemWidth, int cellItemHeight)
        {
            switch (flexgridType)
            {
                case CellSizeStyle.UniformCell:
                    {
                        uniformCellWidth = cellItemWidth;
                        uniformCellHeight = cellItemHeight;
                    } break;
                case CellSizeStyle.UniformHeight:
                    {
                        uniformCellHeight = cellItemHeight;
                    } break;
                case CellSizeStyle.UniformWidth:
                    {
                        uniformCellWidth = cellItemWidth;
                    } break;

            }
        }
        internal GridRowCollection Rows
        {
            get
            {
                return gridRows;
            }
        }
        internal GridColumnCollection Columns
        {
            get
            {
                return gridCols;
            }
        }
        public void AddNewColumn(int initColumnWidth)
        {
            gridCols.Add(new GridColumn(initColumnWidth));
        }
        public void AddColumn(GridColumn col)
        {
            gridCols.Add(col);
        }
        public void InsertColumn(int index, GridColumn col)
        {
            gridCols.Insert(index, col);
        }
        public void InsertRowAfter(GridRow afterThisRow, GridRow row)
        {
            gridRows.InsertAfter(afterThisRow, row);
        }
        public GridColumn GetColumnByPosition(int x)
        {
            return gridCols.GetColumnAtPosition(x);
        }
        public GridRow GetRowByPosition(int y)
        {
            return gridRows.GetRowAtPos(y);
        }

        public void AddRow(GridRow row)
        {
            gridRows.Add(row);
        }

        public GridRow GetRow(int index)
        {
            return gridRows[index];
        }

        public GridColumn GetColumn(int index)
        {
            return gridCols[index];
        }
        public void AddNewRow(int initRowHeight)
        {

            gridRows.Add(new GridRow(initRowHeight));
        }

        public IEnumerable<GridColumn> GetColumnIter()
        {
            return gridCols.GetColumnIter();
        }
        public IEnumerable<GridRow> GetRowIter()
        {
            return gridRows.RowIter;
        }
        public int ColumnCount
        {
            get
            {
                return gridCols.Count;
            }
        }
        public void MoveRowAfter(GridRow fromRow, GridRow toRow)
        {
            this.gridRows.MoveRowAfter(fromRow, toRow);

            owner.InvalidateGraphic();
        }
        public void MoveColumnAfter(GridColumn tobeMoveColumn, GridColumn afterColumn)
        {
            this.gridCols.MoveColumnAfter(tobeMoveColumn, afterColumn);
            owner.InvalidateGraphic();
        }

        public override void TopDownReCalculateContentSize()
        {
#if DEBUG
            vinv_dbug_EnterLayerReCalculateContent(this);
#endif


            if (this.gridRows == null || gridCols.Count < 1)
            {

                SetPostCalculateLayerContentSize(0, 0);
#if DEBUG
                vinv_dbug_ExitLayerReCalculateContent();
#endif
                return;

            }
            //---------------------------------------------------------- 
            //this.BeginReCalculatingContentSize();
            int sumWidth = 0;
            int maxHeight = 0;
            foreach (GridColumn colDef in gridCols.GetColumnIter())
            {

                colDef.ReCalculateColumnSize();

                if (!colDef.HasCustomSize)
                {
                    sumWidth += colDef.DesiredWidth;
                }
                else
                {
                    sumWidth += colDef.Width;
                }

                if (colDef.DesiredHeight > maxHeight)
                {
                    maxHeight = colDef.DesiredHeight;
                }
            }
            foreach (GridRow rowDef in gridRows.RowIter)
            {
                rowDef.CalculateRowHeight();
            }

            if (sumWidth < 1)
            {
                sumWidth = 1;
            }
            if (maxHeight < 1)
            {
                maxHeight = 1;
            }

            SetPostCalculateLayerContentSize(sumWidth, maxHeight);
#if DEBUG
            vinv_dbug_ExitLayerReCalculateContent();
#endif


        }


#if DEBUG
        public override string ToString()
        {

            return "grid layer (L" + dbug_layer_id + this.dbugLayerState + ") postcal:" +
                 this.PostCalculateContentSize.ToString() +
                " of " + owner.dbug_FullElementDescription();
        }

#endif

        public override bool PrepareDrawingChain(VisualDrawingChain chain)
        {

            GridCell leftTopGridItem = GetGridItemByPosition(chain.UpdateAreaX, chain.UpdateAreaY);
            if (leftTopGridItem == null)
            {
                return false;
            }


            GridCell rightBottomGridItem = GetGridItemByPosition(chain.UpdateAreaRight, chain.UpdateAreaBottom);
            if (rightBottomGridItem == null)
            {
                return false;
            }

            GridColumn startColumn = leftTopGridItem.column;
            GridColumn currentColumn = startColumn;
            GridRow startRow = leftTopGridItem.row;
            GridColumn stopColumn = rightBottomGridItem.column.NextColumn;
            GridRow stopRow = rightBottomGridItem.row.NextRow;

            int startRowId = startRow.RowIndex;
            int stopRowId = 0;
            if (stopRow == null)
            {
                stopRowId = gridRows.Count;
            }
            else
            {
                stopRowId = stopRow.RowIndex;
            }

            //----------------------------------------------------------------------------
            //draw border
            //Pen autoBorderPen = new Pen(Color.DarkGray);
            //autoBorderPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            //int n = 0;
            //do
            //{

            //   
            //   
            //    GridItem startGridItemInColumn = currentColumn.GetCell(startRowId);
            //    GridItem stopGridItemInColumn = currentColumn.GetCell(stopRowId - 1);

            //    
            //   
            //    //canvasPage.DrawLine(Color.DarkGray,
            //    //    startGridItemInColumn.RightTopCorner,
            //    //    stopGridItemInColumn.RightBottomCorner);

            //    if (n == 0)
            //    {
            //        
            //        int horizontalLineWidth = rightBottomGridItem.Right - startGridItemInColumn.X;
            //      
            //        
            //        for (int i = startRowId; i < stopRowId; i++)
            //        {
            //           
            //            
            //            GridItem gridItem = currentColumn.GetCell(i);
            //            int x = gridItem.X;
            //            int gBottom = gridItem.Bottom;

            //            
            //            canvasPage.DrawLine(
            //                Color.DarkGray,
            //                x, gBottom,
            //                x + horizontalLineWidth, gBottom);

            //        }
            //        n = 1; 
            //    }
            //   
            //    currentColumn = currentColumn.NextColumn;
            //} while (currentColumn != stopColumn);

            //autoBorderPen.Dispose();

            currentColumn = startColumn;
            //----------------------------------------------------------------------------
            do
            {


                for (int i = startRowId; i < stopRowId; i++)
                {

                    GridCell gridItem = currentColumn.GetCell(i);

                    if (gridItem != null && gridItem.ContentElement != null)
                    {

                        if (gridItem.PrepareDrawingChain(chain))
                        {
                            return true;
                        }

                    }
#if DEBUG
                    //else
                    //{
                    //    canvasPage.DrawText(new char[] { '0' }, gridItem.X, gridItem.Y);
                    //}
#endif


                }

                currentColumn = currentColumn.NextColumn;

            } while (currentColumn != stopColumn);

            return false;

        }
        public override void DrawChildContent(Canvas canvasPage, InternalRect updateArea)
        {
            GridCell leftTopGridItem = GetGridItemByPosition(updateArea._left, updateArea._top);
            if (leftTopGridItem == null)
            {
                return;

            }
            GridCell rightBottomGridItem = GetGridItemByPosition(updateArea._right, updateArea._bottom);
            if (rightBottomGridItem == null)
            {
                return;
            }


            this.BeginDrawingChildContent();

            GridColumn startColumn = leftTopGridItem.column;
            GridColumn currentColumn = startColumn;
            GridRow startRow = leftTopGridItem.row;
            GridColumn stopColumn = rightBottomGridItem.column.NextColumn;
            GridRow stopRow = rightBottomGridItem.row.NextRow;

            int startRowId = startRow.RowIndex;
            int stopRowId = 0;
            if (stopRow == null)
            {
                stopRowId = gridRows.Count;
            }
            else
            {
                stopRowId = stopRow.RowIndex;
            }


            int n = 0;
            do
            {

                GridCell startGridItemInColumn = currentColumn.GetCell(startRowId);
                GridCell stopGridItemInColumn = currentColumn.GetCell(stopRowId - 1);


                canvasPage.DrawLine(Color.DarkGray,
                    startGridItemInColumn.RightTopCorner,
                    stopGridItemInColumn.RightBottomCorner);

                if (n == 0)
                {

                    int horizontalLineWidth = rightBottomGridItem.Right - startGridItemInColumn.X;

                    for (int i = startRowId; i < stopRowId; i++)
                    {

                        GridCell gridItem = currentColumn.GetCell(i);
                        int x = gridItem.X;
                        int gBottom = gridItem.Bottom;


                        canvasPage.DrawLine(
                            Color.DarkGray,
                            x, gBottom,
                            x + horizontalLineWidth, gBottom);

                    }
                    n = 1;
                }
                currentColumn = currentColumn.NextColumn;
            } while (currentColumn != stopColumn);

            currentColumn = startColumn;
            //----------------------------------------------------------------------------
            do
            {


                for (int i = startRowId; i < stopRowId; i++)
                {

                    GridCell gridItem = currentColumn.GetCell(i);

                    if (gridItem != null && gridItem.ContentElement != null)
                    {

                        int x = gridItem.X;
                        int y = gridItem.Y;

                        canvasPage.OffsetCanvasOrigin(x, y);
                        updateArea.Offset(-x, -y);

                        gridItem.DrawToThisPage(canvasPage, updateArea);


                        canvasPage.OffsetCanvasOrigin(-x, -y);
                        updateArea.Offset(x, y);
                    }
#if DEBUG
                    else
                    {
                        canvasPage.DrawText(new char[] { '.' }, gridItem.X, gridItem.Y);
                    }
#endif
                }

                currentColumn = currentColumn.NextColumn;

            } while (currentColumn != stopColumn);
            this.FinishDrawingChildContent();
        }

#if  DEBUG
        public override void dbug_DumpElementProps(dbugLayoutMsgWriter writer)
        {
            writer.Add(new dbugLayoutMsg(this, this.ToString()));
        }
#endif

    }
}