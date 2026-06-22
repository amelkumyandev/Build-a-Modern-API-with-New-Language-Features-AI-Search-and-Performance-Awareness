"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { clearToken } from "@/lib/api";

const nav = [
  ["/admin/dashboard", "Dashboard"],
  ["/admin/documents", "Documents"],
  ["/admin/search", "Search"],
  ["/admin/agent", "Agent"],
  ["/admin/evaluation", "Evaluation"],
  ["/admin/settings", "Settings"]
];

export function AdminShell({ children }: { children: React.ReactNode }) {
  const router = useRouter();

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">Local Knowledge Intelligence</div>
        <nav className="nav">
          {nav.map(([href, label]) => (
            <Link key={href} href={href}>
              {label}
            </Link>
          ))}
          <button
            type="button"
            onClick={() => {
              clearToken();
              router.push("/login");
            }}
          >
            Logout
          </button>
        </nav>
      </aside>
      <main className="main">{children}</main>
    </div>
  );
}
