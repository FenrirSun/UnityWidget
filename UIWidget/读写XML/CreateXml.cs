using System;
using System.IO;
using System.Xml;


public class CreateXml
{
	//Load the XML document
	XmlDocument document = new XmlDocument();
	document.Load(@"C:\...");
	
	//Get the root element
	XmeElement root = document.DocumentElement;
	
	//create the new nodes
	Xmeelement newBook = document.CreateElement("book");
	Xmeelement newTitle = document.CreateElement("title");
	Xmeelement newAuthor = document.CreateElement("author");
	Xmeelement newCode = document.CreateElement("code");
	XmlText title = codument.createTextNode("Beginning Visual C#");
	XmlText author = codument.createTextNode("Karli Watson");
	XmlText code = codument.createTextNode("123456");
	XmlComment comment = document.createComment("The previous edition");
	
	//Insert the elements.
	newBook.AppendChild(comment);
	newBook.AppendChild(newTitle);
	newBook.AppendChild(newAuthor);
	newBook.AppendChild(newCode);
	newTitle.AppendChild(code);
	newAuthor.AppendChild(author);
	newCode.AppendChild(code);
	root.InsertAfter(newBook, root.FirstChild);
	document.Save(@"C:\...");

}
