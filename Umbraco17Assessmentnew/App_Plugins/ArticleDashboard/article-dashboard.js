/**
 * article-dashboard.js
 * Umbraco 17 backoffice dashboard — plain JS Web Component (no build step needed).
 * Fetches article stats from the Management API and renders them
 * using UUI (Umbraco UI Library) components for consistent backoffice styling.
 */

import { UmbElementMixin }    from '@umbraco-cms/backoffice/element-api';
import { UMB_AUTH_CONTEXT }   from '@umbraco-cms/backoffice/auth';
import { LitElement, html, css, nothing } from '@umbraco-cms/backoffice/external/lit';

const API_PATH = '/umbraco/management/api/v1/article-stats';

class ArticleStatsDashboard extends UmbElementMixin(LitElement) {

    static properties = {
        _stats:   { state: true },
        _loading: { state: true },
        _error:   { state: true },
    };

    static styles = css`
        :host {
            display: block;
            padding: var(--uui-size-space-5, 1.25rem);
        }
        h2 { margin-top: 0; font-size: var(--uui-type-h3-size, 1.5rem); }

        .stat-card {
            display: inline-flex;
            flex-direction: column;
            align-items: center;
            background: var(--uui-color-surface-alt, #f4f4f4);
            border-radius: var(--uui-border-radius, 4px);
            padding: 1.25rem 1.5rem;
            margin-bottom: 1.5rem;
            min-width: 140px;
        }
        .stat-card__number {
            font-size: 2.5rem;
            font-weight: 700;
            color: var(--uui-color-interactive, #1b264f);
            line-height: 1;
        }
        .stat-card__label {
            font-size: .875rem;
            color: var(--uui-color-text-alt, #6b7280);
            margin-top: .25rem;
        }

        table { width: 100%; border-collapse: collapse; }
        th, td {
            text-align: left;
            padding: .75rem 1rem;
            border-bottom: 1px solid var(--uui-color-divider, #e5e7eb);
        }
        th { background: var(--uui-color-surface-alt, #f4f4f4); font-weight: 600; }
        tr:last-child td { border-bottom: none; }
        a { color: var(--uui-color-interactive, #1b264f); }

        .error-box {
            color: var(--uui-color-danger, #d00);
            background: #fff0f0;
            padding: 1rem;
            border-radius: 4px;
        }
    `;

    constructor() {
        super();
        this._stats   = null;
        this._loading = true;
        this._error   = null;
    }

    connectedCallback() {
        super.connectedCallback();
        this._fetchStats();
    }

    async _fetchStats() {
        this._loading = true;
        this._error   = null;

        try {
            const authContext = await this.getContext(UMB_AUTH_CONTEXT);
            const config      = await authContext.getOpenApiConfiguration();

            const response = await fetch(API_PATH, {
                headers: {
                    Authorization: `Bearer ${config.token}`,
                    'Content-Type': 'application/json',
                },
            });

            if (!response.ok)
                throw new Error(`API error ${response.status}: ${response.statusText}`);

            this._stats = await response.json();
        } catch (err) {
            this._error = err instanceof Error ? err.message : String(err);
        } finally {
            this._loading = false;
        }
    }

    _fmt(dateStr) {
        return new Intl.DateTimeFormat('en-GB', {
            day: '2-digit', month: 'short', year: 'numeric',
        }).format(new Date(dateStr));
    }

    render() {
        return html`
            <uui-box>
                <h2 slot="headline">📊 Article Stats</h2>

                ${this._loading
                    ? html`<uui-loader-bar></uui-loader-bar>`
                    : nothing}

                ${this._error ? html`
                    <div class="error-box" role="alert">
                        <strong>Could not load stats:</strong> ${this._error}
                    </div>
                    <uui-button look="outline" style="margin-top:1rem"
                        @click=${this._fetchStats}>
                        Retry
                    </uui-button>` : nothing}

                ${this._stats && !this._loading ? html`
                    <div class="stat-card">
                        <span class="stat-card__number">${this._stats.totalArticles}</span>
                        <span class="stat-card__label">Total Articles</span>
                    </div>

                    <h3>Recently Published</h3>
                    <table>
                        <thead>
                            <tr>
                                <th>Title</th>
                                <th>Published</th>
                                <th>View</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${this._stats.recentArticles.map(a => html`
                                <tr>
                                    <td>${a.title}</td>
                                    <td>${this._fmt(a.publishDate)}</td>
                                    <td>
                                        <a href="${a.url}" target="_blank" rel="noopener">
                                            Open ↗
                                        </a>
                                    </td>
                                </tr>
                            `)}
                        </tbody>
                    </table>` : nothing}
            </uui-box>
        `;
    }
}

customElements.define('article-stats-dashboard', ArticleStatsDashboard);
export default ArticleStatsDashboard;
