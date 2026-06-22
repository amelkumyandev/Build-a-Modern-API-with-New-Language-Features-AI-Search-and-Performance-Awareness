import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Local Knowledge Intelligence",
  description: "Admin dashboard for local RAG documents, search, agent chat, and evaluation."
};

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
