   M S B A M L   `   `   `  ����Q  MCommodityinfoToSQLLite, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:CommodityinfoToSQLLiteCommodityinfoToSQLLite  5   	   '    !CommodityinfoToSQLLite.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   6local$clr-namespace:CommodityinfoToSQLLite     5   	   -   5          � Title$  
MainWindow��5   	   $	��180��6   $	��500��6)   .��5	      �  � RowDefinitions 5
   
   �� 5      $
��auto �6   �� 5      $
��auto �6   �� 5      $
��auto �6   �� 5      $
��auto �6   .+�5   
   �� $��0��6    �� Orientation$ 
Horizontal=�6"   $��Center=�6;   $��7,7,0,0q�6V   .+�5      �� $��選取商品檔��6   $	��100��6%   $��Center=�61   ]� 5      .+�5      �� -   5         ��Name$ btChooseCsv��5      $��選取檔案��6.   $
��Left=�6=   $��3q�6S   6_   �� 5      -   5         ��Name$ tbPath��5      $
��auto��6I   $
��Left=�6V   $��3q�6l   ��6*   ��  �� Path$ FilePath�6l   5      5      �� 5   
   $��1��6   $ 
Horizontal=�6"   $��Center=�6;   $��7,3,0,0q�6V   .+�5      �� $��選取SQL Lite位置��6   $	��100��6,   $��Center=�68   ]� 5      .+�5      �� -   5        $ btChooseSQL��5      $��選取檔案��6.   $
��Left=�6=   $��3q�6S   6_   �� 5      -   5        $ 	tbSQLPath��5      $
��auto��6K   $
��Left=�6X   $��3q�6n   ��6-   �� $ SQLPath�6n   5      5      �� 5   
   $��2��6   $ 
Horizontal=�6"   $��0,7,0,0q�6;   .+�5      �� -   5        $ btImportSQL��5      $��商品檔匯入SQL LITE��6*   $	��150��6B   $
��Left=�6N   $��17,0,0,0q�6d   $��3q�6v   6�   �� 5       -   5        $ btImportCsv��5       $��SQL LITE匯入商品檔��6*   $	��150��6B   $
��Left=�6N   $��17,0,0,0q�6d   $��3q�6v   6�   5!      � 5"   
   $��3��6   $��0,10,0,0q�6   .+�5#      � -   5         �Name$ 
pgProgress��5#      $��20��6.    � Minimum$ 0Q�6:    � Maximum 6F   �� $ 
TotalCount�	 � Value	 6j   �� $ Count��� 5$      -	   5        
 ��Name$
 	tbProcess��5$      $��Center=�6I   $��Center=�6f   $��14(�6�   $��17,0,0,0q�6�   ��6+   �� $ Process�6�   5%      5&      5'      