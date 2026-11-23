using System.IO.Compression;
using System.Text;
using System.Xml;
namespace EduFlow.Web.Helpers;
// Tạo XLSX chuẩn bằng thư viện .NET, không đổi đuôi CSV thành Excel.
public static class ExcelExport
{
    public static byte[] Create(string[] headers,IEnumerable<object?[]> rows)
    {
        const string ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        using var memory=new MemoryStream();
        using(var zip=new ZipArchive(memory,ZipArchiveMode.Create,true))
        {
            void Part(string path,string text){using var writer=new StreamWriter(zip.CreateEntry(path).Open(),new UTF8Encoding(false));writer.Write(text);}
            Part("[Content_Types].xml","""<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/><Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/></Types>""");
            Part("_rels/.rels","""<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>""");
            Part("xl/workbook.xml","""<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Bao cao EduFlow" sheetId="1" r:id="rId1"/></sheets></workbook>""");
            Part("xl/_rels/workbook.xml.rels","""<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/><Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/></Relationships>""");
            Part("xl/styles.xml","""<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="11"/><name val="Calibri"/></font></fonts><fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill></fills><borders count="1"><border/></borders><cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs><cellXfs count="4"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/><xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/><xf numFmtId="3" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/><xf numFmtId="2" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/></cellXfs><cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles></styleSheet>""");
            using var xml=XmlWriter.Create(zip.CreateEntry("xl/worksheets/sheet1.xml").Open(),new XmlWriterSettings{Encoding=new UTF8Encoding(false),CloseOutput=true});
            void Start(string name)=>xml.WriteStartElement(name,ns);
            Start("worksheet");Start("sheetViews");Start("sheetView");xml.WriteAttributeString("workbookViewId","0");Start("pane");xml.WriteAttributeString("ySplit","1");xml.WriteAttributeString("topLeftCell","A2");xml.WriteAttributeString("state","frozen");xml.WriteEndElement();xml.WriteEndElement();xml.WriteEndElement();
            Start("cols");Start("col");xml.WriteAttributeString("min","1");xml.WriteAttributeString("max",headers.Length.ToString());xml.WriteAttributeString("width","24");xml.WriteAttributeString("customWidth","1");xml.WriteEndElement();xml.WriteEndElement();
            Start("sheetData");var index=0;
            foreach(var values in new[]{headers.Cast<object?>().ToArray()}.Concat(rows))
            {
                index++;Start("row");xml.WriteAttributeString("r",index.ToString());
                for(var col=0;col<headers.Length;col++)
                {
                    var value=col<values.Length?values[col]:null;var numeric=value is byte or short or int or long or decimal or double or float;
                    Start("c");xml.WriteAttributeString("r",Column(col+1)+index);xml.WriteAttributeString("s",index==1?"1":numeric?((value is decimal fraction && fraction!=decimal.Truncate(fraction)) || value is double or float ? "3":"2"):"0");xml.WriteAttributeString("t",numeric?"n":"inlineStr");
                    if(numeric){Start("v");xml.WriteString(Convert.ToString(value,System.Globalization.CultureInfo.InvariantCulture));xml.WriteEndElement();}
                    else{Start("is");Start("t");xml.WriteAttributeString("xml","space",null,"preserve");var text=value is DateTime date?date.ToString("yyyy-MM-dd HH:mm"):value?.ToString()??"";xml.WriteString(System.Text.RegularExpressions.Regex.Replace(text,@"[\x00-\x08\x0B\x0C\x0E-\x1F]",""));xml.WriteEndElement();xml.WriteEndElement();}xml.WriteEndElement();
                }
                xml.WriteEndElement();
            }
            xml.WriteEndElement();Start("autoFilter");xml.WriteAttributeString("ref",$"A1:{Column(headers.Length)}{index}");xml.WriteEndElement();xml.WriteEndElement();
        }
        return memory.ToArray();
    }
    private static string Column(int number){var name="";while(number>0){number--;name=(char)('A'+number%26)+name;number/=26;}return name;}
}

