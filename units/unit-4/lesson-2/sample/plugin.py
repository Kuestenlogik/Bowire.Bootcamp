"""Drop-in replacement for the MyProtocol class the
``dotnet new bowire-plugin --Sidecar python`` scaffold emits. Used
in Lesson 06 as the "your turn — make the sidecar actually do
something" edit.

Renames the demo service+method to a Yoda-Speak theme and swaps the
scaffold's echo ``invoke`` for a tiny English-to-Yoda translator so
users see their own code on the response pane — and so the contrast
with Lesson 05's .NET Pirate plugin is obvious (same SDK surface,
totally different language).
"""

import json

from bowire_plugin import (
    BowirePlugin,
    FieldInfo,
    InvokeResult,
    MessageInfo,
    MethodInfo,
    ServiceInfo,
)


class MyProtocol(BowirePlugin):
    """Yoda Speak — a Bowire sidecar plugin written in Python."""

    id = "yoda"
    name = "Yoda Speak"

    def discover(self, server_url: str, show_internal: bool):
        """Return the topology Bowire renders in the sidebar."""
        translate_input = MessageInfo(
            name="TranslateRequest",
            full_name="yoda.TranslateRequest",
            fields=[
                FieldInfo(
                    name="text",
                    type="string",
                    required=True,
                    description="The text to convert into Yoda-speak.",
                ),
            ],
        )
        translate_output = MessageInfo(
            name="TranslateResponse",
            full_name="yoda.TranslateResponse",
            fields=[FieldInfo(name="translated", type="string")],
        )
        return [
            ServiceInfo(
                name="JediCouncil",
                description="A long time ago, in a JSON envelope far, far away.",
                methods=[
                    MethodInfo(
                        name="Translate",
                        method_type="Unary",
                        input_type=translate_input,
                        output_type=translate_output,
                        summary="Convert plain English into Yoda-speak.",
                    ),
                ],
            )
        ]

    def invoke(self, server_url, service, method, json_messages,
               show_internal, metadata):
        """Dispatch a unary call. ``json_messages`` is the request list
        (one entry for unary methods, multiple for client-streaming)."""
        payload = json_messages[0] if json_messages else "{}"
        try:
            text = json.loads(payload).get("text", "")
        except json.JSONDecodeError:
            text = ""

        translated = _yodify(text)
        return InvokeResult(
            response=json.dumps({"translated": translated}),
            status="OK",
        )


def _yodify(text: str) -> str:
    """Very small reversal — proves the plugin's response really comes
    from your code, not from the scaffold. Replace with an actual
    wire call (HTTP, MQTT, NATS, anything Python can talk to) when
    you build a real protocol plugin.
    """
    if not text:
        return "Wisdom you seek, hmmmm."
    words = text.split()
    if len(words) < 2:
        return f"{text}, hmmmm."
    return " ".join(reversed(words)) + ", hmmmm."
