import { html as v, css as h, property as d, customElement as m } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as _ } from "@umbraco-cms/backoffice/lit-element";
import { UmbChangeEvent as f } from "@umbraco-cms/backoffice/event";
var g = Object.defineProperty, E = Object.getOwnPropertyDescriptor, l = (e) => {
  throw TypeError(e);
}, u = (e, t, a, n) => {
  for (var r = n > 1 ? void 0 : n ? E(t, a) : t, i = e.length - 1, o; i >= 0; i--)
    (o = e[i]) && (r = (n ? o(t, a, r) : o(r)) || r);
  return n && r && g(t, a, r), r;
}, y = (e, t, a) => t.has(e) || l("Cannot " + a), x = (e, t, a) => t.has(e) ? l("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, a), S = (e, t, a) => (y(e, t, "access private method"), a), c, p;
let s = class extends _ {
  constructor() {
    super(...arguments), x(this, c), this.value = "";
  }
  render() {
    const e = this.value?.length ?? 0;
    return v`
            <uui-textarea label="Message" .value=${this.value ?? ""} @input=${S(this, c, p)}></uui-textarea>
            <div class="count">${e} ${e === 1 ? "character" : "characters"}</div>
        `;
  }
};
c = /* @__PURE__ */ new WeakSet();
p = function(e) {
  this.value = e.target.value, this.dispatchEvent(new f());
};
s.styles = h`
        :host {
            display: block;
        }
        .count {
            margin-top: var(--uui-size-space-2, 6px);
            color: var(--uui-color-text-alt, #68676b);
            font-size: var(--uui-type-small-size, 12px);
        }
    `;
u([
  d({ type: String })
], s.prototype, "value", 2);
s = u([
  m("ua-kitchen-sink-message-editor")
], s);
const U = s;
export {
  s as UaKitchenSinkMessageEditorElement,
  U as default
};
//# sourceMappingURL=message-editor.element-CDi_nkRf.js.map
